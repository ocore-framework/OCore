namespace OCore.Diagnostics.C4.Attributes;

public class C4PersonAttribute : Attribute
{
    public string Name { get; }
    
    public C4PersonAttribute(string name)
    {
        Name = name;
    }
}