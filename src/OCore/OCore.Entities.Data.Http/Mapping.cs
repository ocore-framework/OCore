using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OCore.Authorization.Abstractions;
using OCore.Core;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace OCore.Entities.Data.Http
{
    public static class Mapping
    {
        public static IEndpointRouteBuilder MapDataEntities(this IEndpointRouteBuilder routes, string prefix = "")
        {
            var payloadCompleter = routes.ServiceProvider.GetRequiredService<IPayloadCompleter>();
            var dataEntitiesToMap = DiscoverDataEntitiesToMap();

            int routesCreated = 0;
            // Map each grain type to a route based on the attributes
            foreach (var serviceType in dataEntitiesToMap)
            {
                routesCreated += MapDataEntityToRoute(routes, serviceType, prefix, payloadCompleter);
            }

            return routes;
        }

        private static IEnumerable<Type> GetAllTypesThatImplementInterface<T>()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => type.IsInterface
                               && type.GetCustomAttribute<GeneratedCodeAttribute>() == null
                               && type.GetInterfaces().Contains(typeof(T)));
        }

        private static List<Type> DiscoverDataEntitiesToMap()
        {
            var iDataEntityImplementors = GetAllTypesThatImplementInterface<IDataEntity>().ToList();
            var thatHaveDataEntityAttribute = iDataEntityImplementors.Where(t => t.GetCustomAttributes(true)
                .Where(attr => attr.GetType() == typeof(DataEntityAttribute)).SingleOrDefault() != null).ToList();
            return thatHaveDataEntityAttribute;
        }

        private static int MapDataEntityToRoute(IEndpointRouteBuilder routes, Type grainType, string prefix,
            IPayloadCompleter payloadCompleter)
        {
            var methods = grainType.GetMethods();
            int routesRegistered = 0;

            var dataEntityName = grainType.FullName;

            var dataEntityAttribute = (DataEntityAttribute)grainType.GetCustomAttributes(true)
                .Where(attr => attr.GetType() == typeof(DataEntityAttribute)).SingleOrDefault();

            var keyStrategy = KeyStrategy.Identity;
            var dataEntityMethods = DataEntityMethods.All;
            var maxFanoutLimit = 0;

            if (dataEntityAttribute != null)
            {
                dataEntityName = dataEntityAttribute.Name;
                keyStrategy = dataEntityAttribute.KeyStrategy;
                dataEntityMethods = dataEntityAttribute.DataEntityMethods;
                maxFanoutLimit = dataEntityAttribute.MaxFanoutLimit;
            }

            var loggerFactory = routes.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("OCore.Entities.Data.Http.Mapping");

            logger.LogInformation("Mapping routes for DataEntity '{DataEntityName}'", dataEntityName);
            
            routesRegistered += MapCustomMethods(dataEntityName, keyStrategy, maxFanoutLimit, routes, payloadCompleter,
                prefix, methods, routesRegistered);
            routesRegistered += MapCrudMethods(dataEntityName, grainType, keyStrategy, maxFanoutLimit,
                dataEntityMethods, routes, payloadCompleter, prefix, routesRegistered);

            return routesRegistered;
        }

        private static int MapCustomMethods(string dataEntityName,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            IEndpointRouteBuilder routeBuilder,
            IPayloadCompleter payloadCompleter,
            string prefix,
            MethodInfo[] methods,
            int routesRegistered)
        {
            var loggerFactory = routeBuilder.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("OCore.Entities.Data.Http.Mapping");

            foreach (var method in methods)
            {
                var internalAttribute = method.GetCustomAttribute<InternalAttribute>(true);

                if (internalAttribute == null)
                {
                    DataEntityMethodDispatcher.Register(routeBuilder,
                        prefix,
                        dataEntityName,
                        keyStrategy,
                        maxFanoutLimit,
                        payloadCompleter,
                        method.DeclaringType,
                        method);


                    logger.LogInformation(" => '{DataEntityName}': {MethodName}", dataEntityName, method.Name);

                    routesRegistered++;
                }
            }

            return routesRegistered;
        }

        private static int MapCrudMethods(string dataEntityName,
            Type declaringType,
            KeyStrategy keyStrategy,
            int maxFanoutLimit,
            DataEntityMethods dataEntityMethods,
            IEndpointRouteBuilder routeBuilder,
            IPayloadCompleter payloadCompleter,
            string prefix,
            int routesRegistered)
        {
            var loggerFactory = routeBuilder.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("OCore.Entities.Data.Http.Mapping");

            var dataEntityType = (
                from iType in declaringType.GetInterfaces()
                where iType.IsGenericType
                      && iType.GetGenericTypeDefinition() == typeof(IDataEntity<>)
                select iType.GetGenericArguments()[0]).FirstOrDefault();

            if (dataEntityType == null)
            {
                return 0;
            }

            void Register(HttpMethod httpMethod)
            {
                DataEntityCrudDispatcher.Register(routeBuilder,
                    prefix,
                    dataEntityName,
                    keyStrategy,
                    maxFanoutLimit,
                    declaringType,
                    dataEntityType,
                    payloadCompleter,
                    httpMethod);
                
                logger.LogInformation(" => '{DataEntityName}': {MethodName}", dataEntityName, httpMethod);
            }


            if (dataEntityMethods.HasFlag(DataEntityMethods.Create))
            {
                Register(HttpMethod.Post);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.Read))
            {
                Register(HttpMethod.Get);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.Update))
            {
                Register(HttpMethod.Put);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.Delete))
            {
                Register(HttpMethod.Delete);
                routesRegistered++;
            }

            if (dataEntityMethods.HasFlag(DataEntityMethods.PartialUpdate))
            {
                Register(HttpMethod.Patch);
                routesRegistered++;
            }

            return routesRegistered;
        }
    }
}