using System.Reflection;
using OCore.Authorization.Request;

namespace OCore.Resources.ResourceTypes;

[Serializable]
[GenerateSerializer]
public class ServiceResource : Resource
{
    public ServiceResource(string resourceName,
        string resourceType,
        Permissions permission,
        MethodInfo methodInfo,
        bool isPublic)
        : base(resourceName,
            resourceType,
            permission,
            methodInfo,
            isPublic)
    {
    }
}