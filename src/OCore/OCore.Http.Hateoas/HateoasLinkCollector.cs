using Microsoft.AspNetCore.Http;

namespace OCore.Http.Hateoas;

/// <summary>
/// Somewhat naive implementation of a HATEOAS link collector
/// </summary>
public class HateoasLinkCollector
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HateoasLinkCollector(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public IEnumerable<HateoasLink> GetLinks()
    {
        var links = AddSelf();

        return links;
    }

    public string FormatTemplate(string template)
    {
        // Assume the template starts with a path if there is no scheme
        if (template.StartsWith("/"))
        {
            template += "{scheme}://{host}";
        }
        
        var request = _httpContextAccessor.HttpContext.Request;
        var path = request.Path.Value;
        var method = request.Method;
        var host = request.Host.Value;
        var scheme = request.Scheme;
        
        template = template.Replace("{scheme}", scheme)
            .Replace("{host}", host)
            .Replace("{path}", path)
            .Replace("{method}", method);
        
        return template;
    }

    private List<HateoasLink> AddSelf()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        var path = request.Path.Value;
        var method = request.Method;
        var host = request.Host.Value;
        var scheme = request.Scheme;

        var links = new List<HateoasLink>();
        links.Add(new HateoasLink
        {
            Href = $"{scheme}://{host}{path}",
            Rel = "self",
            Method = method
        });
        return links;
    }
}