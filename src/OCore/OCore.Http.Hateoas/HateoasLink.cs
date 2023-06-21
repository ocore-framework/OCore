namespace OCore.Http.Hateoas;

public class HateoasLink
{
    public string Href { get; set; } = string.Empty;

    public string Rel { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;
}