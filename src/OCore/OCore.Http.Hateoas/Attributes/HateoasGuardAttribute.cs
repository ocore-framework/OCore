namespace OCore.Http.Hateoas.Attributes;

/// <summary>
/// Rub this attribute on a bool property that indicates whether a link should be added to the response 
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
public class HateoasGuardAttribute : Attribute
{
    public HttpMethod HttpMethod { get; }
    public string? HrefTemplate { get; }

    public string? Command { get; set; }
    
    /// <summary>
    /// Self links 
    /// </summary>
    /// <param name="httpMethod"></param>
    public HateoasGuardAttribute(HttpMethod httpMethod)
    {
        HttpMethod = httpMethod;
    }

    public HateoasGuardAttribute(string command)
    {
        Command = command;
    }
    
    /// <summary>
    /// More complex links, supporting templating
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <param name="hrefTemplate">The HREF template. Certain keys will be interpolated.</param>
    public HateoasGuardAttribute(HttpMethod httpMethod, string hrefTemplate)
    {
        HttpMethod = httpMethod;
        HrefTemplate = hrefTemplate; 
    }
}