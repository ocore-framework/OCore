using Microsoft.AspNetCore.Http;
using Orleans;

namespace OCore.Http;

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
        ContentType = httpContext.Request.ContentType;
        Host = httpContext.Request.Host.Value;
    }


    [Id(0)] public string Path { get; set; }

    [Id(1)] public string Scheme { get; set; }
    
    [Id(2)] public string Method { get; set; }
    
    [Id(3)] public string? QueryString { get; set; }
    
    [Id(5)] public string? ContentType { get; set; }
    
    [Id(6)] public string Host { get; set; }
    
    public override string ToString()
    {
        return $"{Method} {Scheme}{Path}{QueryString}, ContentType: {ContentType}";
    }
}