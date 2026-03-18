using System.Globalization;
using System.Resources;

namespace Clingies.Resources;

internal static class IconPathResources
{
    private static readonly ResourceManager ResourceManager =
        new("Clingies.Resources.IconPathResources", typeof(IconPathResources).Assembly);

    public static string? GetPath(string iconId, bool isDarkPath = false)
    {
        var variant = isDarkPath ? "dark" : "light";
        return ResourceManager.GetString($"{iconId}_{variant}", CultureInfo.InvariantCulture);
    }
}
