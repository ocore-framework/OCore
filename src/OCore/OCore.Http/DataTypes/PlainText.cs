using Orleans;

namespace OCore.Http.DataTypes;

[GenerateSerializer]
public class PlainText
{
    [Id(0)] public string Text { get; set; }
}