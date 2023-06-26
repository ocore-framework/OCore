namespace OCore.Diagnostics.C4.Attributes;

public class C4SoftwareSystemAttribute : Attribute
{
    public string Name { get; }

    public C4SoftwareSystemAttribute(string name)
    {
        Name = name;
    }
}