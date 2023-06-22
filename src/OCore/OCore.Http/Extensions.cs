using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using IActionFilter = OCore.Http.Filters.IActionFilter;
using IAsyncActionFilter = OCore.Http.Filters.IAsyncActionFilter;

namespace OCore.Http
{
    public static class Extensions
    {

        static readonly ConcurrentDictionary<MethodInfo, Func<HttpContext, Task>> asyncFilters = new();

        public static async Task RunAsyncActionFilters(this HttpContext context,
            GrainInvoker invoker,
            Func<HttpContext, Task> next)
        {

            if (asyncFilters.TryGetValue(invoker.MethodInfo, out var storedFilters))
            {
                await storedFilters(context);
            }
            else
            {

                var filters = invoker.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<IAsyncActionFilter>()
                    .OrderBy(x => x.Order)
                    .ToArray();

                Func<HttpContext, Task> Wrap(Filters.IAsyncActionFilter filter, Func<HttpContext, Task> n)
                {
                    return (context) => filter.OnActionExecutionAsync(context, n);
                }

                Func<HttpContext, Task> BuildCallChain(int i, Filters.IAsyncActionFilter filter, Func<HttpContext, Task> n)
                {
                    i--;
                    if (i == 0)
                    {
                        return Wrap(filters[i], n);
                    }
                    return BuildCallChain(i, filters[i], Wrap(filters[i], n));
                }

                int filterCount = filters.Length;
                Func<HttpContext, Task> callChain;
                if (filterCount == 0)
                {
                    callChain = next;
                }
                else
                {
                    callChain = BuildCallChain(filterCount, filters[filterCount - 1], next);
                    asyncFilters.AddOrUpdate(invoker.MethodInfo, callChain, (key, oldValue) => callChain);
                }
                await callChain(context);
            }
        }

        static ConcurrentDictionary<MethodInfo, IEnumerable<IAuthorizationFilter>> authorizationFilters = new();
        static ConcurrentDictionary<MethodInfo, IEnumerable<Filters.IActionFilter>> actionFilters = new();

        public static void RunActionFiltersExecuting(this HttpContext context, GrainInvoker invoker)
        {
            if (actionFilters.TryGetValue(invoker.MethodInfo, out var storedFilters))
            {
                RunActionFiltersExecuting(context, storedFilters);
            }
            else
            {
                var filters = invoker.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<IActionFilter>()
                    .OrderBy(x => x.Order);
                actionFilters.AddOrUpdate(invoker.MethodInfo, filters, (key, oldvalue) => filters);
                RunActionFiltersExecuting(context, filters);
            }
        }

        private static void RunActionFiltersExecuting(HttpContext context, IEnumerable<Filters.IActionFilter> filters)
        {
            foreach (var filter in filters)
            {
                filter.OnActionExecuting(context);
            }
        }

        private static void RunActionFiltersExecuted(HttpContext context, IEnumerable<Filters.IActionFilter> filters)
        {
            foreach (var filter in filters)
            {
                filter.OnActionExecuted(context);
            }
        }

        public static void RunActionFiltersExecuted(this HttpContext context, GrainInvoker invoker)
        {
            if (actionFilters.TryGetValue(invoker.MethodInfo, out var storedFilters))
            {
                RunActionFiltersExecuted(context, storedFilters);
            }
            else
            {

                var filters = invoker.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<IActionFilter>()
                    .OrderBy(x => x.Order);
                actionFilters.AddOrUpdate(invoker.MethodInfo, filters, (key, oldvalue) => filters);
                RunActionFiltersExecuted(context, filters);
            }
        }

        public static void RunAuthorizationFilters(this HttpContext context, GrainInvoker invoker)
        {
            var actionContext = new ActionContext(context, new RouteData(), new ActionDescriptor());
            var authorizationFilterContext = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

            if (authorizationFilters.TryGetValue(invoker.MethodInfo, out var filters))
            {
                RunAuthorizationFilters(authorizationFilterContext, filters);
            }
            else
            {
                var attributes = invoker.MethodInfo.GetCustomAttributes(true)
                    .Concat(invoker.GrainType.GetCustomAttributes(true));
                filters = attributes
                    .OfType<IAuthorizationFilter>();
                authorizationFilters.AddOrUpdate(invoker.MethodInfo, filters, (key, oldvalue) => filters);
                RunAuthorizationFilters(authorizationFilterContext, filters);
            }
        }

        private static void RunAuthorizationFilters(this AuthorizationFilterContext authorizationFilterContext, IEnumerable<IAuthorizationFilter> filters)
        {
            foreach (var filter in filters)
            {
                filter.OnAuthorization(authorizationFilterContext);
            }
        }

        public static async Task SetStatusCode(this HttpContext context, HttpStatusCode statusCode, string message = null)
        {
            context.Response.StatusCode = (int)statusCode;
            if (message != null)
            {
                await context.Response.WriteAsync(message);
            }
        }
    }
}
