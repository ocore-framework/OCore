using System.Reflection;
using OCore.Authorization.Abstractions;

namespace OCore.Resources;

public abstract class Resource
{
    /// <summary>
    /// The specialized name of the resource
    /// </summary>
    public string ResourceName { get; private set; }

    /// <summary>
    /// The base resource that holds this specialization
    /// </summary>
    public string ResourceType { get; private set; }

    /// <summary>
    /// Permissions needed to reach this concrete resource
    /// </summary>
    public Permissions Permissions { get; private set; }


    public MethodInfo MethodInfo { get; private set; }

    public bool IsPublic { get; private set; }

    public Resource(string resourceName,
        string resourceType,
        Permissions permission,
        MethodInfo methodInfo,
        bool isPublic)
    {
        ResourceName = resourceName;
        Permissions = permission;
        ResourceType = resourceType;
        MethodInfo = methodInfo;
        IsPublic = isPublic;
    }
}
