namespace OCore.Diagnostics.C4.Attributes;

public class C4ContainerAttribute : Attribute
{
    public string Name { get; }

    public C4ContainerAttribute(string name)
    {
        Name = name;
    }
}