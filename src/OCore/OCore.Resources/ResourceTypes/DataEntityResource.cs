using System.Reflection;
using OCore.Authorization.Request;
using OCore.Entities.Data;

namespace OCore.Resources.ResourceTypes;

[Serializable]
[GenerateSerializer]
public class DataEntityResource : Resource
{
    [Id(0)]
    public DataEntityAttribute Attribute { get; private set; }

    public DataEntityResource(string resourceName,
        string baseResource,
        Permissions permission,
        MethodInfo methodInfo,
        DataEntityAttribute attribute,
        bool isPublic)
        : base(resourceName, baseResource, permission, methodInfo, isPublic)
    {
        Attribute = attribute;
    }
}
