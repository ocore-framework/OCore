using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orleans;
using Orleans.Runtime;

namespace OCore.Http.Middleware;

#nullable enable

/// <summary>
/// Map of HttpContext to something that can be serialized and sent with the RequestContext
/// </summary>
[GenerateSerializer]
public class HttpContextRequest
{
    public HttpContextRequest(HttpContext httpContext)
    {
        Path = httpContext.Request.Path;
        Scheme = httpContext.Request.Scheme;
        Method = httpContext.Request.Method;
        QueryString = httpContext.Request.QueryString.Value;
        Body = httpContext.Request.Body.ToString();
        ContentType = httpContext.Request.ContentType;
    }


    [Id(0)] public string Path { get; set; }

    [Id(1)] public string Scheme { get; set; }
    
    [Id(2)] public string Method { get; set; }
    
    [Id(3)] public string? QueryString { get; set; }
    
    [Id(4)] public string? Body { get; set; }
    
    [Id(5)] public string? ContentType { get; set; }
    
    public override string ToString()
    {
        return $"Path: {Path}, Scheme: {Scheme}, Method: {Method}, QueryString: {QueryString}, Body: {Body}, ContentType: {ContentType}";
    }
}

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