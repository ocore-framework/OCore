using Microsoft.AspNetCore.Http;
using OCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace OCore.Entities.Data.Http
{
    public enum HttpMethod
    {
        NotSet,
        Get,
        Post,
        Put,
        Delete,
        Patch,
    }

    public class DataEntityGrainInvoker : GrainInvoker
    {
        Type entityType;

        public DataEntityGrainInvoker(IServiceProvider serviceProvider, Type grainType, MethodInfo methodInfo,
            Type entityType) :
            base(serviceProvider, grainType, methodInfo)
        {
            this.entityType = entityType;
        }

        public bool IsCrudOperation { get; set; }

        public HttpMethod HttpMethod { get; set; }

        protected override object[] GetParameterList(string body)
        {
            if (IsCrudOperation == false)
            {
                return GetCallParameters(body);
            }
            else
            {
                return GetCrudParameters(body);
            }
        }

        object[] GetCrudParameters(string body)
        {
            if (HttpMethod == HttpMethod.NotSet)
            {
                throw new InvalidOperationException("HttpMethod is not set for CRUD operation");
            }

            switch (HttpMethod)
            {
                case HttpMethod.Get:
                case HttpMethod.Delete:
                    return new object[] { };
                case HttpMethod.Post:
                case HttpMethod.Put:
                case HttpMethod.Patch:
                    return GetCrudBodyEntityParameters(body);
                default:
                    throw new InvalidOperationException("HttpMethod is not set for known CRUD operation");
            }
        }

        private object[] GetCrudBodyEntityParameters(string body)
        {
            return new object[] { JsonSerializer.Deserialize(body, entityType) };
        }

        private async Task<string[]> GetBodyKeys(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body);
            var body = await reader.ReadToEndAsync();
            
            JsonDocument jsonDocument = JsonDocument.Parse(body);

            // Get the root element of the JSON document
            JsonElement root = jsonDocument.RootElement;

            // Iterate over the properties and print the keys
            var enumerator = root.EnumerateObject();
            var count = enumerator.Count();
            
            var keys = new string[count];
            var i = 0;
            
            foreach (JsonProperty property in root.EnumerateObject())
            {
                keys[i] = property.Name;
            }

            return keys;
        }

        object[] GetCallParameters(string body)
        {
            var parameterList = new List<object>();

            if (string.IsNullOrEmpty(body) == true)
            {
                AddDefaultParameters(parameterList);
            }
            else if (body[0] == '[')
            {
                var deserialized = JsonSerializer.Deserialize<object[]>(body);

                if (deserialized.Length > Parameters.Count)
                {
                    throw new InvalidOperationException($"Parameter count too high");
                }

                int i = 0;
                foreach (var parameter in deserialized)
                {
                    parameterList.Add(ProjectValue(parameter, Parameters[i++]));
                }
                AddDefaultParameters(parameterList);
            }
            else
            {
                if (Parameters.Count != 1)
                {
                    throw new InvalidOperationException($"Parameter count mismatch");
                }

                parameterList.Add(JsonSerializer.Deserialize(body, Parameters[0].Type));
            }

            if (HttpMethod == HttpMethod.Patch)
            {
                parameterList.Add(GetCrudBodyEntityParameters(body));
            }
            
            return parameterList.ToArray();
        }

        private void AddDefaultParameters(List<object> parameterList)
        {
            while (parameterList.Count < Parameters.Count)
            {
                parameterList.Add(Type.Missing);
            }
        }
    }
}