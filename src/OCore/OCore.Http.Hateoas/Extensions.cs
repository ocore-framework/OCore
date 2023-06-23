using System.Reflection;
using OCore.Entities.Data;
using OCore.Http.Hateoas.Attributes;
using Orleans.Runtime;

namespace OCore.Http.Hateoas;

public static class Extensions
{
    public static IEnumerable<HateoasLink> GetHateoasLinks<T>(this T entity)
        where T : IIdentifyable
    {
        var httpRequest = RequestContext.Get("HttpContextRequest") as HttpContextRequest;

        // This should be runnable and testable without a HttpContextRequest
        if (httpRequest is null)
        {
            return Enumerable.Empty<HateoasLink>();
        }

        List<HateoasLink> links = new();

        // Get the IDataEntity<T>-derived interface from the data entity
        Type? interfaceType = typeof(T).GetInterfaces()
            .FirstOrDefault(i => i.GetCustomAttributes(typeof(DataEntityAttribute), true).Any());

        if (interfaceType is not null)
        {
            // Get DataEntity-attribute from the data entity
            var dataEntityAttribute = interfaceType.GetCustomAttribute<DataEntityAttribute>();

            if (dataEntityAttribute is not null)
            {
                if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Create))
                {
                    links.Add(new HateoasLink()
                    {
                        Rel = "self",
                        Href = FormatTemplate("{scheme}://{host}{path}", entity.Id, httpRequest),
                        Method = "POST"
                    });
                }
                if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Delete))
                {
                    links.Add(new HateoasLink()
                    {
                        Rel = "self",
                        Href = FormatTemplate("{scheme}://{host}{path}", entity.Id, httpRequest),
                        Method = "DELETE"
                    });
                }
                if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Update))
                {
                    links.Add(new HateoasLink()
                    {
                        Rel = "self",
                        Href = FormatTemplate("{scheme}://{host}{path}", entity.Id, httpRequest),
                        Method = "PUT"
                    });
                }
                if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.PartialUpdate))
                {
                    links.Add(new HateoasLink()
                    {
                        Rel = "self",
                        Href = FormatTemplate("{scheme}://{host}{path}", entity.Id, httpRequest),
                        Method = "PATCH"
                    });
                }
                if (dataEntityAttribute.DataEntityMethods.HasFlag(DataEntityMethods.Read))
                {
                    links.Add(new HateoasLink()
                    {
                        Rel = "self",
                        Href = FormatTemplate("{scheme}://{host}{path}", entity.Id, httpRequest),
                        Method = "GET"
                    });
                }
            }
        }

        // Get all bool-properties on entity that have the HateoasGuardAttribute and evaluate to false
        var propertyAttributes = typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(bool))
            .Select(p => new { Property = p, Attributes = p.GetCustomAttributes<HateoasGuardAttribute>() })
            .Where(p => (bool)p.Property.GetValue(entity)! == false);

        // If any properties were found with the HateoasGuardAttribute, remove the corresponding links
        foreach (var property in propertyAttributes)
        {
            foreach (var attribute in property.Attributes)
            {
                links.RemoveAll(l =>
                    l.Rel == "self"
                    && l.Method == attribute.HttpMethod.ToString().ToUpper());
            }
        }
        
        var commands = propertyAttributes.SelectMany(p => p.Attributes)
            .Where(a => a.Command is not null)
            .Select(a => a.Command)
            .Distinct();
        
        // Add commands from interfaceType
        if (interfaceType is not null)
        {
            var methods = interfaceType.GetMethods();
               

            foreach (var method in methods)
            {
                var methodName = method.Name;
                if (commands.Any(x => x == methodName)) continue;
                
                links.Add(new HateoasLink()
                {
                    Rel = "self",
                    Href = FormatTemplate("{scheme}://{host}{path}/" + method.Name, entity.Id, httpRequest),
                    Method = HttpMethod.Post.ToString().ToUpper()
                });
            }
        }
        
        return links;
    }

    public static string FormatTemplate(string template, string id, HttpContextRequest request)
    {
        var path = request.Path;
        var method = request.Method;
        var host = request.Host;
        var scheme = request.Scheme;

        template = template.Replace("{scheme}", scheme)
            .Replace("{host}", host)
            .Replace("{path}", path)
            .Replace("{id}", id);

        return template;
    }
}