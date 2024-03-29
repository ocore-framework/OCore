﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OCore.Http;
using Orleans;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using OCore.Authorization.Request;
using OCore.Authorization.Request.Abstractions;
using Orleans.Runtime;

namespace OCore.Entities.Data.Http
{
    public class DataEntityMethodDispatcher : DataEntityDispatcher
    {
        DataEntityGrainInvoker invoker;
        MethodInfo methodInfo;
        IClusterClient clusterClient;
        IPayloadCompleter payloadCompleter;
        Type grainType;

        public DataEntityMethodDispatcher(
            IEndpointRouteBuilder routeBuilder,
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            int maxFanOutLimit,
            IPayloadCompleter payloadCompleter,
            Type grainType,
            MethodInfo methodInfo) :
            base(prefix, dataEntityName, keyStrategy, maxFanOutLimit)
        {
            this.grainType = grainType;
            this.methodInfo = methodInfo;
            this.payloadCompleter = payloadCompleter;
            clusterClient = routeBuilder.ServiceProvider.GetRequiredService<IClusterClient>();

            invoker = new DataEntityGrainInvoker(routeBuilder.ServiceProvider, grainType, methodInfo, null);
            routeBuilder.MapPost(GetRoutePattern(methodInfo.Name).RawText, Dispatch);
        }

        public async Task Dispatch(HttpContext httpContext)
        {
            var endpoint = (RouteEndpoint)httpContext.GetEndpoint();
            var pattern = endpoint.RoutePattern;

            RequestContext.Set("D:RequestSource", "HTTP");
            RequestContext.Set("D:GrainName", pattern.RawText);
            
            httpContext.RunAuthorizationFilters(invoker);
            httpContext.RunActionFiltersExecuting(invoker);
            await httpContext.RunAsyncActionFilters(invoker, async (context) =>
            {
                var payload = Payload.GetOrDefault();
                if (payload != null)
                {
                    await payloadCompleter.Complete(payload, clusterClient);
                }

                var grainKeys = GetKeys(httpContext);
                if (grainKeys.Length == 0)
                {
                    await httpContext.SetStatusCode(System.Net.HttpStatusCode.BadRequest, "Unreachable destination");
                    httpContext.RunActionFiltersExecuted(invoker);
                    return;
                }

                if (grainKeys.Length == 1)
                {
                    var grain = clusterClient.GetGrain(grainType, grainKeys[0]);
                    if (grain == null)
                    {
                        await httpContext.SetStatusCode(System.Net.HttpStatusCode.BadRequest, "Unreachable destination");
                        httpContext.RunActionFiltersExecuted(invoker);
                        return;
                    }

                    using var reader = new StreamReader(context.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    
                    await invoker.Invoke(grain, httpContext, body);
                    httpContext.RunActionFiltersExecuted(invoker);
                }
                else
                {

                }
            });
        }

        public static DataEntityMethodDispatcher Register(IEndpointRouteBuilder routeBuilder,
            string prefix,
            string dataEntityName,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            IPayloadCompleter payloadCompleter,
            Type grainType,
            MethodInfo methodInfo)
        {
            return new DataEntityMethodDispatcher(routeBuilder,
                prefix,
                dataEntityName,
                keyStrategy,
                maxFanoutLimit,
                payloadCompleter,
                grainType,
                methodInfo);
        }

    }
}
