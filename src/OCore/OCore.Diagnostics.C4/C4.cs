using System.Text.RegularExpressions;
using OCore.Resources;

namespace OCore.Diagnostics.C4;

// This will come soon
public static class C4
{
    // If you want to implement both "*" and "?"
    private static String WildcardToRegex(String value) {
        return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$"; 
    }
    
    public static string GenerateC4Model(string systemName, 
        string resourceInclusionWildcard = "*",
        string? regex = null)
    {
        if (regex is null)
        {
            regex = WildcardToRegex(resourceInclusionWildcard);
        }

        var resources = ResourceEnumerator.PublicResources
            .Where(r => Regex.IsMatch(r.ResourceType, regex));

        ;
        return "";
    }
    
}