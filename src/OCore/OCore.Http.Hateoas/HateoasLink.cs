namespace OCore.Http.Hateoas;

[GenerateSerializer]
public class HateoasLink
{
    [Id(0)] public string Href { get; set; } = string.Empty;

    [Id(1)] public string Rel { get; set; } = string.Empty;

    [Id(2)] public string Method { get; set; } = string.Empty;
}