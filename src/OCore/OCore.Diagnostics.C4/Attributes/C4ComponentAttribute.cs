namespace OCore.Diagnostics.C4.Attributes;

public class C4ComponentAttribute : Attribute
{
    public string Name { get; }

    public C4ComponentAttribute(string name)
    {
        Name = name;
    }
}