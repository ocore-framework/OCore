using System.Reflection;
using System.Text.RegularExpressions;
using OCore.Resources;

namespace OCore.Diagnostics.C4;

// This will come soon
public static class C4
{
    private static String WildcardToRegex(String value)
    {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
    }

    public static string GenerateC4Model(
        this Assembly assembly,
        string containerName,
        int systemNamespaceDepth = 2,
        string resourceInclusionWildcard = "*",
        string? regex = null)
    {
        if (regex is null)
        {
            regex = WildcardToRegex(resourceInclusionWildcard);
        }

        var resources = ResourceEnumerator.PublicResourcesWithoutOCore
            .Where(r => Regex.IsMatch(r.ResourceType, regex));

        ;
        return "";
    }
}