﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections.Features;
using OCore.Http;

namespace OCore.Setup
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            if (RequestContext.Get("D:CorrelationId") is string correlationId)
            {
                context.Response.Headers.Add("CorrelationId", correlationId);
            }

            if (ex is StatusCodeException sce)
            {
                code = sce.StatusCode;
            }
            else if (ex is UnauthorizedAccessException uae)
            {
                code = HttpStatusCode.Unauthorized;
            }
            else if (ex is ArgumentException ae)
            {
                code = HttpStatusCode.BadRequest;
            }

            var result = JsonConvert.SerializeObject(new { error = ex.ToString() });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }
    }
}