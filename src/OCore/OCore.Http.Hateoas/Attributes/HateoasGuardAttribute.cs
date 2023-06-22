namespace OCore.Http.Hateoas.Attributes;

/// <summary>
/// Rub this attribute on a bool property that indicates whether a link should be added to the response 
/// </summary>
public class HateoasGuardAttribute : Attribute
{
    public string Rel { get; }
    public string HttpMethod { get; }
    public string? HrefTemplate { get; }

    /// <summary>
    /// Self links 
    /// </summary>
    /// <param name="httpMethod"></param>
    public HateoasGuardAttribute(string httpMethod)
    {
        HttpMethod = httpMethod;
        Rel = "self";
    }
    
    /// <summary>
    /// More complex links, supporting templating
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <param name="rel"></param>
    /// <param name="hrefTemplate">The HREF template. Certain keys will be interpolated.</param>
    public HateoasGuardAttribute(string httpMethod, string rel, string hrefTemplate)
    {
        HttpMethod = httpMethod;
        Rel = rel;
        HrefTemplate = hrefTemplate; 
    }
}