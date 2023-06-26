using OCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace OCore.Services.Http
{
    class Parameter
    {
        public Type Type { get; set; }
        public string Name { get; set; }
    }

    public class ServiceGrainInvoker : GrainInvoker
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true
        };

        public ServiceGrainInvoker(IServiceProvider serviceProvider, Type grainType, MethodInfo methodInfo) :
            base(serviceProvider, grainType, methodInfo)
        {
        }

        protected override object[] GetParameterList(string body)
        {
            var parameterList = new List<object>();

            if (string.IsNullOrEmpty(body) == true)
            {
                AddDefaultParameters(parameterList);
            }
            else if (body[0] == '[')
            {
                var deserialized = JsonSerializer.Deserialize<object[]>(body, jsonSerializerOptions);

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

                parameterList.Add(JsonSerializer.Deserialize(body, Parameters[0].Type, jsonSerializerOptions));
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