using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using OCore.Core;
using OCore.Entities.Data;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OCore.Resources;
using OCore.Resources.ResourceTypes;

namespace OCore.Http.OpenApi
{
    public class OpenApiHandler
    {
        string Title { get; set; }
        string Version { get; set; }
        bool LoadDocumentationXml { get; set; }
        bool StripInternal { get; set; }
        string servicePrefix { get; set; }
        string dataEntityPrefix { get; set; }
        
        string[] openApiInternalPrefixes { get; set; }

        public OpenApiHandler(string title,
            string version,
            bool stripInternal,
            string[] openApiInternalPrefixes = null,
            bool loadDocumentationXml = true,
            string servicePrefix = "/services",
            string dataEntityPrefix = "/data")
        {
            Title = title;
            Version = version;
            StripInternal = stripInternal;
            LoadDocumentationXml = loadDocumentationXml;
            this.servicePrefix = servicePrefix;
            this.dataEntityPrefix = dataEntityPrefix;
            this.openApiInternalPrefixes = openApiInternalPrefixes;
        }

        internal async Task Dispatch(HttpContext context)
        {
            try
            {
                OpenApiDocument document = CreateDocument();
                context.Response.ContentType = "application/json";

                var openApiDocumentation = document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json);

                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(openApiDocumentation), 0,
                    openApiDocumentation.Length);
                await context.Response.Body.FlushAsync();
            }
            catch (Exception ex)
            {
                var exc = ex.ToString();
                await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(exc), 0, exc.Length);
            }
        }

        private OpenApiDocument CreateDocument()
        {
            var assemblyLocation = Assembly.GetEntryAssembly().Location;

            var resourceList = ResourceEnumerator.PublicResources;
            var resolver = new JsonSerializerDataContractResolver(new JsonSerializerOptions());
            var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), resolver);
            var schemaRepository = new SchemaRepository();

            var apiPaths = CreateApiPaths(resourceList, schemaGenerator, schemaRepository);

            return new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = Version,
                    Title = Title,
                },
                Servers = new List<OpenApiServer>
                {
                    new() { Url = "http://localhost:9000" }
                },
                Paths = apiPaths,
                Components = new OpenApiComponents()
                {
                    Schemas = schemaRepository.Schemas
                }
            };
        }

        private OpenApiPaths CreateApiPaths(List<Resource> resourceList, SchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository)
        {
            var paths = new OpenApiPaths();

            Dictionary<string, List<Resource>> baseResources = new Dictionary<string, List<Resource>>();

            foreach (var resourceEntry in resourceList)
            {
                if (baseResources.TryGetValue(resourceEntry.ResourceType, out var resourceDescriptions))
                {
                    resourceDescriptions.Add(resourceEntry);
                }
                else
                {
                    baseResources.Add(resourceEntry.ResourceType, new List<Resource> { resourceEntry });
                }
            }

            var orderedResourceList = OrderResourceList(resourceList);
            
            foreach (var resource in orderedResourceList)
            {
                if (StripInternal == true
                    && resource.ResourceType.StartsWith("OCore")
                    && openApiInternalPrefixes.Contains(resource.ResourceType) == false) continue;
                if (resource is ServiceResource serviceResource)
                {
                    if (serviceResource.MethodInfo.GetCustomAttribute<InternalAttribute>() != null)
                    {
                        continue;
                    }

                    AddServiceResource(paths, serviceResource, servicePrefix, schemaGenerator, schemaRepository);
                }
                else if (resource is DataEntityResource dataEntityResource)
                {
                    if (dataEntityResource.MethodInfo.GetCustomAttribute<InternalAttribute>() != null)
                    {
                        continue;
                    }

                    AddDataEntityResource(paths, dataEntityResource, dataEntityPrefix, schemaGenerator,
                        schemaRepository);
                }
            }

            return paths;
        }

        // Split into OCore-related resources and "other" and push out OCore-stuff at the end
        private IEnumerable<Resource> OrderResourceList(List<Resource> resourceList)
        {
            var orderedResourceList = resourceList.OrderBy(x => x.ResourceType);
            var ocoreResources = orderedResourceList.Where(x => x.ResourceType.StartsWith("OCore")).ToList();
            var otherResources = orderedResourceList.Where(x => x.ResourceType.StartsWith("OCore") == false).ToList();
            
            return otherResources.Concat(ocoreResources);
        }

        private void AddDataEntityResource(OpenApiPaths paths, DataEntityResource dataEntityResource,
            string dataEntityPrefix, SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            if (dataEntityResource.ResourceType != dataEntityResource.ResourceName)
            {
                var parameterString = CreateParameterString(dataEntityResource);
                paths.Add(
                    $"{dataEntityPrefix}/{dataEntityResource.ResourceType}/{{id}}/{dataEntityResource.MethodInfo.Name}",
                    new OpenApiPathItem
                    {
                        Parameters = new List<OpenApiParameter>()
                        {
                            new()
                            {
                                Name = "id",
                                Schema = new OpenApiSchema() { Type = "string" },
                                Required = true,
                                In = ParameterLocation.Path,
                                Description = "Id of the data entity"
                            }
                        },
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            {
                                OperationType.Post, new OpenApiOperation
                                {
                                    RequestBody = GetRequestType(dataEntityResource.MethodInfo, schemaGenerator,
                                        schemaRepository),
                                    Tags = new List<OpenApiTag>
                                    {
                                        new() { Name = dataEntityResource.ResourceType, Description = "DataEntity" }
                                    },
                                    Description = dataEntityResource.ResourceType,
                                    Summary =
                                        $"{dataEntityResource.ResourceType}.{dataEntityResource.MethodInfo.Name}({parameterString})",
                                    Responses = new OpenApiResponses
                                    {
                                        ["200"] = GetResponseType(dataEntityResource.MethodInfo, schemaGenerator,
                                            schemaRepository)
                                    },
                                    Parameters = new List<OpenApiParameter>()
                                    {
                                        new()
                                        {
                                            Name = "id",
                                            Schema = new OpenApiSchema() { Type = "string" },
                                            Required = true,
                                            In = ParameterLocation.Path,
                                            Description = "Id of the data entity"
                                        }
                                    },
                                }
                            }
                        }
                    });
            }
            else
            {
                if (paths.ContainsKey(dataEntityResource.ResourceType) == false)
                {
                    var operations = new Dictionary<OperationType, OpenApiOperation>();
                    var dataEntityInterface = dataEntityResource.MethodInfo.DeclaringType.GenericTypeArguments[0];

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Read))
                    {
                        var responses = new OpenApiResponses
                        {
                            ["200"] = GetDataEntityStateResponseType(dataEntityResource.MethodInfo, schemaGenerator,
                                schemaRepository),
                            ["404"] = new() { Description = "Not found" },
                        };
                        operations.Add(OperationType.Get,
                            new OpenApiOperation
                            {
                                Responses = responses,
                                Tags = new List<OpenApiTag>
                                {
                                    new() { Name = dataEntityResource.ResourceType, Description = "DataEntity" }
                                },
                                Description = dataEntityResource.ResourceType,
                                Summary = $"{dataEntityResource.ResourceType}.{DataEntityMethods.Read}",
                                Parameters = new List<OpenApiParameter>()
                                {
                                    new()
                                    {
                                        Name = "id",
                                        Schema = new OpenApiSchema() { Type = "string" },
                                        Required = true,
                                        In = ParameterLocation.Path,
                                        Description = "Id of the data entity"
                                    }
                                },
                            });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Create))
                    {
                        operations.Add(OperationType.Post,
                            new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag>
                                    { new() { Name = dataEntityResource.ResourceType, Description = "DataEntity" } },
                                Description = dataEntityResource.ResourceType,
                                Summary = $"{dataEntityResource.ResourceType}.{DataEntityMethods.Create}",
                                RequestBody = GetRequestType(dataEntityResource.MethodInfo, schemaGenerator,
                                    schemaRepository),
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new() { Description = "OK" },
                                    ["409"] = new() { Description = "Conflict if DataEntity is already created" }
                                },
                                Parameters = new List<OpenApiParameter>()
                                {
                                    new()
                                    {
                                        Name = "id",
                                        Schema = new OpenApiSchema() { Type = "string" },
                                        Required = true,
                                        In = ParameterLocation.Path,
                                        Description = "Id of the data entity"
                                    }
                                },
                            });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Update))
                    {
                        operations.Add(OperationType.Put,
                            new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag>
                                {
                                    new() { Name = dataEntityResource.ResourceType, Description = "DataEntity" }
                                },
                                Description = dataEntityResource.ResourceType,
                                Summary = $"{dataEntityResource.ResourceType}.{DataEntityMethods.Update}",
                                RequestBody = GetRequestType(dataEntityResource.MethodInfo, schemaGenerator,
                                    schemaRepository),
                                Parameters = new List<OpenApiParameter>()
                                {
                                    new()
                                    {
                                        Name = "id",
                                        Schema = new OpenApiSchema() { Type = "string" },
                                        Required = true,
                                        In = ParameterLocation.Path,
                                        Description = "Id of the data entity"
                                    }
                                },
                            });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.Delete))
                    {
                        operations.Add(OperationType.Delete,
                            new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag>
                                    { new() { Name = dataEntityResource.ResourceType, Description = "DataEntity" } },
                                Description = dataEntityResource.ResourceType,
                                Summary = $"{dataEntityResource.ResourceType}.{DataEntityMethods.Delete}",
                                Parameters = new List<OpenApiParameter>()
                                {
                                    new()
                                    {
                                        Name = "id",
                                        Schema = new OpenApiSchema() { Type = "string" },
                                        Required = true,
                                        In = ParameterLocation.Path,
                                        Description = "Id of the data entity"
                                    }
                                },
                            });
                    }

                    if (dataEntityResource.Attribute.DataEntityMethods.HasFlag(DataEntityMethods.PartialUpdate))
                    {
                        operations.Add(OperationType.Patch,
                            new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag>
                                {
                                    new() { Name = dataEntityResource.ResourceType, Description = "DataEntity" }
                                },
                                Description = dataEntityResource.ResourceType,
                                RequestBody = GetRequestType(dataEntityResource.MethodInfo, schemaGenerator,
                                    schemaRepository),
                                Summary = $"{dataEntityResource.ResourceType}.{DataEntityMethods.PartialUpdate}",
                                Parameters = new List<OpenApiParameter>()
                                {
                                    new()
                                    {
                                        Name = "id",
                                        Schema = new OpenApiSchema() { Type = "string" },
                                        Required = true,
                                        In = ParameterLocation.Path,
                                        Description = "Id of the data entity"
                                    }
                                },
                            });
                    }

                    paths.Add($"{dataEntityPrefix}/{dataEntityResource.ResourceType}/{{id}}", new OpenApiPathItem
                    {
                        Parameters = new List<OpenApiParameter>()
                            { new() { Name = "id", Schema = new OpenApiSchema() { Type = "string" } } },
                        Operations = operations
                    });
                }
            }
        }

        private static void AddServiceResource(OpenApiPaths paths, Resource serviceResource, string servicePrefix,
            SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            var parameterString = CreateParameterString(serviceResource);

            paths.Add($"{servicePrefix}/{serviceResource.ResourceName}", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    {
                        OperationType.Post, new OpenApiOperation
                        {
                            Tags = new List<OpenApiTag>
                                { new() { Name = serviceResource.ResourceType, Description = "Service" } },
                            Description = serviceResource.ResourceType,
                            RequestBody = GetRequestType(serviceResource.MethodInfo, schemaGenerator, schemaRepository),
                            Summary =
                                $"{serviceResource.ResourceType}.{serviceResource.MethodInfo.Name}({parameterString})",
                            Responses = new OpenApiResponses
                            {
                                ["200"] = GetResponseType(serviceResource.MethodInfo, schemaGenerator, schemaRepository)
                            },
                        }
                    }
                },
            });
        }

        private static string CreateParameterString(Resource resource)
        {
            ParameterInfo[] parameters = resource.MethodInfo.GetParameters();

            var parametersList = parameters
                .Select(x => $"{(x.IsOptional ? "[" : "")}{x.ParameterType.Name} {x.Name}{(x.IsOptional ? "]" : "")}")
                .ToList();

            var parameterString = String.Join(", ", parametersList);
            return parameterString;
        }

        private static OpenApiRequestBody GetRequestType(MethodInfo methodInfo, SchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository)
        {
            var firstParameter = methodInfo.GetParameters().Where(type =>
                !type.ParameterType.IsPrimitive &&
                !type.ParameterType.IsSubclassOf(typeof(System.ValueType)) &&
                !type.ParameterType.Equals(typeof(string))).FirstOrDefault();
            if (firstParameter != null)
            {
                schemaGenerator.GenerateSchema(firstParameter.ParameterType, schemaRepository, methodInfo,
                    firstParameter);
                OpenApiSchema schema;
                schemaRepository.TryLookupByType(firstParameter.ParameterType, out schema);
                var body = new OpenApiRequestBody();
                var json = new OpenApiMediaType { Schema = schema };
                body.Content["application/json"] = json;
                return body;
            }
            else
            {
                var schema = new OpenApiSchema
                {
                    Type = "array",
                    Properties = new Dictionary<string, OpenApiSchema>(),
                    Required = new SortedSet<string>(),
                    AdditionalPropertiesAllowed = true
                };

                var body = new OpenApiRequestBody();
                body.Content["application/json"] = new OpenApiMediaType { Schema = schema };
                return body;
            }
        }

        private static Type GetDataEntityInterfaceType(MethodInfo methodInfo)
        {
            Type entityInterface = methodInfo.DeclaringType;
            if (methodInfo.ReflectedType.GenericTypeArguments.Any())
            {
                entityInterface = methodInfo.ReflectedType.GenericTypeArguments.FirstOrDefault();
            }

            var dataInterface = entityInterface.GetInterfaces().FirstOrDefault(type => type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IDataEntity<>) &&
                typeof(IDataEntity).IsAssignableFrom(type));

            return dataInterface;
        }

        private static OpenApiResponse GetDataEntityStateResponseType(MethodInfo methodInfo,
            SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            var dataEntityInterface = GetDataEntityInterfaceType(methodInfo);
            return GetOrGenerateResponseType(dataEntityInterface.GenericTypeArguments.FirstOrDefault(), methodInfo,
                schemaGenerator, schemaRepository);
        }

        private static OpenApiResponse GetResponseType(MethodInfo methodInfo, SchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository)
        {
            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault();
            if (returnType == null)
            {
                return new OpenApiResponse() { Description = "No content" };
            }

            return GetOrGenerateResponseType(returnType, methodInfo, schemaGenerator, schemaRepository);
        }

        private static OpenApiResponse GetOrGenerateResponseType(Type type, MethodInfo methodInfo,
            SchemaGenerator schemaGenerator, SchemaRepository schemaRepository)
        {
            schemaGenerator.GenerateSchema(type, schemaRepository, methodInfo);
            OpenApiSchema schema;
            schemaRepository.TryLookupByType(type, out schema);

            var response = new OpenApiResponse();
            response.Content["application/json"] = new OpenApiMediaType
            {
                Schema = schema
            };
            response.Description = type.Name;
            return response;
        }
    }
}