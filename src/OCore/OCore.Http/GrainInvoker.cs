﻿using Microsoft.AspNetCore.Http;
using Orleans;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OCore.Http.DataTypes;

namespace OCore.Http
{
    public class Parameter
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    public abstract class GrainInvoker
    {
        public MethodInfo MethodInfo { get; set; }
        protected MethodInfo GetResult { get; set; }

        private static readonly MethodInfo getResultMethod =
            typeof(GrainInvoker).GetMethod(nameof(GetResultDelegate), BindingFlags.Static | BindingFlags.NonPublic);

        private static object GetResultDelegate<T>(Task<T> input) => input.GetAwaiter().GetResult();

        IServiceProvider serviceProvider;

        public Type GrainType { get; private set; }

        protected List<Parameter> Parameters = new List<Parameter>();


        public GrainInvoker(IServiceProvider serviceProvider, Type grainType, MethodInfo methodInfo)
        {
            this.serviceProvider = serviceProvider;
            GrainType = grainType;
            MethodInfo = methodInfo;

            BuildParameterMap();
            BuildResultDelegate();
        }


        protected virtual void BuildResultDelegate()
        {
            if (MethodInfo.ReturnType.IsGenericType
                && MethodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                GetResult = getResultMethod.MakeGenericMethod(MethodInfo.ReturnType.GenericTypeArguments[0]);
            }
        }

        protected virtual void BuildParameterMap()
        {
            var parameters = MethodInfo.GetParameters();

            foreach (var parameter in parameters)
            {
                Parameters.Add(new Parameter
                {
                    Name = parameter.Name,
                    Type = parameter.ParameterType
                });
            }
        }


        public async Task Invoke(IGrain grain, HttpContext context, string body)
        {
            object[] parameterList = GetParameterList(body);
            Task grainCall;
            grainCall = (Task)MethodInfo.Invoke(grain, parameterList);

            await grainCall;

            if (context.Response.Headers.ContainsKey("CorrelationId") == false
                && RequestContext.Get("D:CorrelationId") is string correlationId)
            {
                context.Response.Headers.Add("CorrelationId", correlationId);
            }

            if (GetResult != null)
            {
                object result = GetResult.Invoke(null, new[] { grainCall });
                if (result != null)
                {
                    if (result is PlainText plainText)
                    {
                        context.Response.ContentType = "text/plain";
                        await WriteStringToResponse(context, plainText.Text);
                    }
                    else
                    {
                        context.Response.ContentType = "application/json";
                        await Serialize(result, context.Response.BodyWriter);
                    }
                }
            }
        }

        public async Task WriteStringToResponse(HttpContext context, string content)
        {
            // Get the response body writer
            var responseBodyWriter = context.Response.BodyWriter;

            // Convert the string to bytes using a specific encoding (e.g., UTF-8)
            var contentBytes = Encoding.UTF8.GetBytes(content);

            // Write the content bytes to the response body writer
            await responseBodyWriter.WriteAsync(contentBytes);

            // Indicate that writing is complete
            await responseBodyWriter.CompleteAsync();
        }

        public async Task Invoke(IGrain[] grains, HttpContext context, string body)
        {
            object[] parameterList = GetParameterList(body);
            List<Task> grainCalls = new List<Task>();
            foreach (var grain in grains)
            {
                grainCalls.Add((Task)MethodInfo.Invoke(grain, parameterList));
            }

            try
            {
                await Task.WhenAll(grainCalls);
            }
            // This is supposed to be catch-all, the exceptions are checked below
            catch
            {
            }

            List<object> results = new List<object>();

            foreach (var task in grainCalls)
            {
                if (task.IsCompletedSuccessfully)
                {
                    if (GetResult != null)
                    {
                        object result = GetResult.Invoke(null, new[] { task });
                        results.Add(result);
                    }
                    else
                    {
                        results.Add(null);
                    }
                }
                else if (task.IsFaulted)
                {
                    results.Add(null);
                }
            }

            context.Response.ContentType = "application/json";

            if (context.Response.Headers.ContainsKey("CorrelationId") == false
                && RequestContext.Get("D:CorrelationId") is string correlationId)
            {
                context.Response.Headers.Add("CorrelationId", correlationId);
            }

            await Serialize(results, context.Response.BodyWriter);
        }

        public async ValueTask Serialize(object obj, PipeWriter writer)
        {
            await JsonSerializer.SerializeAsync(writer.AsStream(), obj, obj.GetType());
        }

        protected abstract object[] GetParameterList(string body);

        static Dictionary<Type, Func<object, object>> Converters = new()
        {
            { typeof(string), s => s.ToString() },
            { typeof(int), s => int.Parse(s.ToString(), CultureInfo.InvariantCulture) },
            { typeof(DateTime), s => DateTime.Parse(s.ToString()) },
            { typeof(DateTimeOffset), s => DateTimeOffset.Parse(s.ToString()) },
            { typeof(TimeSpan), s => TimeSpan.Parse(s.ToString()) },
            { typeof(double), s => double.Parse(s.ToString(), CultureInfo.InvariantCulture) },
            { typeof(float), s => float.Parse(s.ToString(), CultureInfo.InvariantCulture) },
            { typeof(decimal), s => decimal.Parse(s.ToString(), CultureInfo.InvariantCulture) }
        };

        protected object ProjectValue(object deserializedValue, Parameter parameter)
        {
            if (Converters.TryGetValue(parameter.Type, out var converter))
            {
                return converter(deserializedValue);
            }
            else
            {
                // This is no doubt a hack. Let's leave it as is for now
                var serialized = JsonSerializer.Serialize(deserializedValue);
                return JsonSerializer.Deserialize(serialized, parameter.Type);
            }
        }
    }
}