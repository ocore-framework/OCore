using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orleans.Runtime;

namespace OCore.Http.Middleware;

/// <summary>
/// ASP.NET Core middleware that provides the current HttpContext to RequestContext
/// </summary>
public class HttpContextMiddleware
{
    readonly RequestDelegate next;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    public HttpContextMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    /// <summary>
    /// Invokes the middleware
    /// </summary>
    /// <param name="context">The current HttpContext</param>
    public async Task Invoke(HttpContext context)
    {
        RequestContext.Set("HttpContextRequest", new HttpContextRequest(context));
        await next(context);
    }
}