using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OCore.Services.Http.Options;
using Orleans.Runtime;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OCore.Diagnostics.Middleware
{
    public class CorrelationIdProviderMiddleware
    {
        private readonly RequestDelegate next;

        public CorrelationIdProviderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IOptions<HttpOptions> options /* other dependencies */)
        {
            // TODO: Should this consider getting the correlationId from the RequestContext?
            
            string? correlationId = null;
            
            var correlationIdKeyName = options.Value.CorrelationIdHeader ?? "correlationId";

            if (context.Request.Headers.TryGetValue(correlationIdKeyName, out var correlationIdHeader))
            {
                correlationId = correlationIdHeader.FirstOrDefault();
            }

            if (correlationId == null)
            {
                correlationId = Guid.NewGuid().ToString();
            }

            RequestContext.Set("D:CorrelationId", correlationId);

            await next(context);
        }
    }
}
