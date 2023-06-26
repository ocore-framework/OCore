using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OCore.Authorization.Request;
using OCore.Authorization.Request.Abstractions;
using OCore.Core;
using OCore.Http;
using Orleans;
using Orleans.Runtime;

namespace OCore.Entities.Data.Http
{
    record DispatchInfo(MethodInfo MethodInfo,
        DataEntityAttribute DataEntityAttribute,
        Type GrainType,
        Type DataEntityType)
    {
        public MethodInfo GetResult { get; set; }
        
        private static readonly MethodInfo getResultMethod = typeof(GrainInvoker).GetMethod(nameof(GetResultDelegate), BindingFlags.Static | BindingFlags.NonPublic);
        private static object GetResultDelegate<T>(Task<T> input) => input.GetAwaiter().GetResult();

        public void BuildResultDelegate()
        {
            if (MethodInfo.ReturnType.IsGenericType
                && MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                GetResult = getResultMethod.MakeGenericMethod(MethodInfo.ReturnType.GenericTypeArguments[0]);
            }
        }
    }

    /// <summary>
    /// A dispatcher that looks for the "," character in the DataEntity-name. It only supports GET, but should
    /// support the following patterns:
    ///
    /// GET .../data/MyDataEntity,MyDataEntity2/1         -- Multiple data entities, single key 
    /// GET .../data/MyDataEntity,MyDataEntity2/1,2,3     -- Multiple data entities, multiple keys
    /// </summary>
    public class DataEntityFanOutDispatcher
    {
        private static readonly MethodInfo getResultMethod = typeof(GrainInvoker).GetMethod(nameof(GetResultDelegate), BindingFlags.Static | BindingFlags.NonPublic);
        private static object GetResultDelegate<T>(Task<T> input) => input.GetAwaiter().GetResult();
        
        private readonly Dictionary<string, DispatchInfo> _entityMap = new();

        private readonly IClusterClient _clusterClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IPayloadCompleter _payloadCompleter;

        public DataEntityFanOutDispatcher(IEnumerable<Type> grainTypes,
            IClusterClient clusterClient,
            IServiceProvider serviceProvider,
            IPayloadCompleter payloadCompleter)
        {
            _clusterClient = clusterClient;
            _serviceProvider = serviceProvider;
            _payloadCompleter = payloadCompleter;
            BuildMap(grainTypes);
        }

        private void BuildMap(IEnumerable<Type> grainTypes)
        {
            foreach (var grainType in grainTypes)
            {
                var internalAttribute =
                    grainType.GetCustomAttributes(typeof(InternalAttribute), true).FirstOrDefault() as
                        InternalAttribute;
                if (internalAttribute is not null) continue;

                var dataEntityAttribute =
                    grainType.GetCustomAttributes(typeof(DataEntityAttribute), true).FirstOrDefault() as
                        DataEntityAttribute;
                if (dataEntityAttribute is null) continue;
                if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Read) == false) continue;

                var methodInfo = typeof(IDataEntity<>).MakeGenericType(grainType).GetMethod("Read");

                internalAttribute = methodInfo.GetCustomAttributes(typeof(InternalAttribute), true).FirstOrDefault() as
                    InternalAttribute;
                if (internalAttribute is not null) continue;

                var dataEntityType = (grainType.GetInterfaces()
                    .Where(iType => iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDataEntity<>))
                    .Select(iType => iType.GetGenericArguments()[0])).FirstOrDefault();

                var dispatchInfo = new DispatchInfo(methodInfo, dataEntityAttribute, grainType, dataEntityType);
                dispatchInfo.BuildResultDelegate();
                
                _entityMap.Add(dataEntityAttribute.Name,
                    dispatchInfo);
            }
        }



        public async Task Dispatch(HttpContext context)
        {
            var identityKeys = GetIdentityKeys(context);
            var dataEntityNames = GetDataEntityNamesFromRoute(context);

            Dictionary<string, GrainInvoker> grainInvokers = new();

            if (identityKeys.Length == 1)
            {
                foreach (var dataEntityName in dataEntityNames)
                {
                    if (string.IsNullOrEmpty(dataEntityName))
                    {
                        throw new StatusCodeException(HttpStatusCode.BadRequest, "Malformed request URI");
                    }
                    
                    if (_entityMap.TryGetValue(dataEntityName, out var dispatchInfo) == false)
                    {
                        throw new StatusCodeException(HttpStatusCode.BadRequest, "One of the data entities not found");
                    }

                    // TODO: Convert identity keys
                    
                    grainInvokers.Add(dataEntityName, new DataEntityGrainInvoker(_serviceProvider,
                        dispatchInfo.GrainType,
                        dispatchInfo.MethodInfo,
                        dispatchInfo.DataEntityType));
                }

                var payload = Payload.GetOrDefault();
                if (payload != null)
                {
                    await _payloadCompleter.Complete(payload, _clusterClient);
                }

                // DataEntity fan-out currently doesn't support asynchronous action filters
                RunPreFilters(context, grainInvokers.Values);
                
                var grainCalls = new Dictionary<string, Task>();
                var parameterList = new object[0];
                foreach (var dispatchInfo in grainInvokers)
                {
                    var grain = _clusterClient.GetGrain(dispatchInfo.Value.GrainType, identityKeys[0]);
                    var invokeTask = (Task)dispatchInfo.Value.MethodInfo.Invoke(grain, parameterList);
                    grainCalls.Add(dispatchInfo.Key, invokeTask);
                }

                try
                {
                    await Task.WhenAll(grainCalls.Values);
                } catch { }

                var output = new Dictionary<string, object>();
                
                foreach (var task in grainCalls)
                {
                    if (task.Value.IsCompletedSuccessfully == true)
                    {
                        var di = _entityMap[task.Key];
                        if (di.GetResult != null)
                        {
                            object result = di.GetResult.Invoke(null, new[] { task.Value });
                            output.Add(task.Key, result);
                        }
                        else
                        {
                            output.Add(task.Key, null);
                        }
                    } 
                }
                
                RunPostFilters(context, grainInvokers.Values);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;

                if (RequestContext.Get("D:CorrelationId") is string correlationId)
                {
                    if (context.Response.Headers.ContainsKey("CorrelationId") == false)
                    {
                        context.Response.Headers.Add("CorrelationId", correlationId);
                    }
                }

                await Serialize(output, context.Response.BodyWriter);
            }
        }
        
        public async ValueTask Serialize(object obj, PipeWriter writer)
        {
            await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType());
        }

        private void RunPreFilters(HttpContext context, IEnumerable<GrainInvoker> invokers)
        {
            foreach (var invoker in invokers)
            {
                context.RunAuthorizationFilters(invoker);
                context.RunActionFiltersExecuting(invoker);
            }
        }

        private void RunPostFilters(HttpContext context, IEnumerable<GrainInvoker> invokers)
        {
            foreach (var invoker in invokers)
            {
                context.RunActionFiltersExecuted(invoker);
            }
        }

        private List<IGrain> GetGrainTypesForIdentity(string identity, string[] dataEntityNames)
        {
            var grains = new List<IGrain>();
            int i = 1;
            foreach (var dataEntityName in dataEntityNames)
            {
                if (_entityMap.TryGetValue(dataEntityName, out var dispatchInfo) == false)
                {
                    grains.Add(_clusterClient.GetGrain(dispatchInfo.GrainType, identity));
                }
                else
                {
                    throw new InvalidOperationException($"Data entity at position {i} not found");
                }

                i++;
            }

            return grains;
        }

        private string[] GetIdentityKeys(HttpContext context)
        {
            var identitesFromRoute = context.Request.RouteValues["identity"].ToString();
            var identities = identitesFromRoute.Split(',');
            return identities;
        }

        private string[] GetDataEntityNamesFromRoute(HttpContext context)
        {
            var dataEntityNamesFromRoute = context.Request.RouteValues["dataEntityNames"].ToString();
            var dataEntityNames = dataEntityNamesFromRoute.Split(',');
            return dataEntityNames;
        }

        private static string GetIdentityFromRoute(HttpContext context)
        {
            return context.Request.RouteValues["identity"].ToString();
        }

        string[] GetIdentities(string identity)
        {
            return identity.Split(',');
        }
    }
}