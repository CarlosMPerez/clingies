using System.Collections.Generic;
using Clingies.Application.Services;
using Gtk;
namespace Clingies.GtkFront.Services;

public sealed class ClingyStyleDto
{
    public string BodyColorHex { get; init; } = AppConstants.SystemStyle.BodyColor;
    public string BodyFontName { get; init; } = AppConstants.SystemStyle.BodyFontName;
    public string BodyFontColorHex { get; init; } = AppConstants.SystemStyle.BodyFontColor;
    public int BodyFontSizePx { get; init; } = AppConstants.SystemStyle.BodyFontSize;
    public Enums.FontDecorations Decorations { get; init; } = Enums.FontDecorations.None;
}

public sealed class ClingyStylingService(StyleService styleService)
{
    private readonly Dictionary<int, CssProvider> _providers = new();

    public void ApplyTo(Widget rootOfNote, int clingyId, int styleId)
    {
        string cls = $"clingy-{clingyId}";
        // Tag the root widget with a unique class
        rootOfNote.StyleContext.AddClass(cls);

        // Build CSS scoped to that class
        string css = BuildCss(cls, styleId, styleService);

        // Get or create provider
        if (!_providers.TryGetValue(clingyId, out var prov))
        {
            prov = new CssProvider();
            _providers[clingyId] = prov;
            // Attach provider; priority User ensures it wins over theme + base app CSS
            StyleContext.AddProviderForScreen(rootOfNote.Screen, prov, StyleProviderPriority.User);
        }

        // Load/Reload CSS (live-updates styles)
        prov.LoadFromData(css);
    }

    public void RemoveFrom(Widget rootOfNote, int clingyId)
    {
        if (_providers.TryGetValue(clingyId, out var prov))
        {
            // Optional: detach
            StyleContext.RemoveProviderForScreen(rootOfNote.Screen, prov);
            _providers.Remove(clingyId);
        }
        rootOfNote.StyleContext.RemoveClass($"clingy-{clingyId}");
    }

    private static string BuildCss(string scopeClass, int styleId, StyleService styleService)
    {
        var style = styleService.Get(styleId);
        // Map flags -> CSS fragments
        var bold = "";  //style.Decorations.HasFlag(Enums.FontDecorations.Bold) ? "font-weight: bold;" : "";
        var italic = "";  //style.Decorations.HasFlag(Enums.FontDecorations.Italic) ? "font-style: italic;" : "";
        var underline = "";  //style.Decorations.HasFlag(Enums.FontDecorations.Underline) ? "text-decoration: underline;" : "";
        var strike = "";  //style.Decorations.HasFlag(Enums.FontDecorations.Strikethrough) ? "text-decoration: line-through;" : "";
        // If both underline & strike, combine:
        var textDeco = ""; //(underline, strike) switch
        // {
        //     ("text-decoration: underline;", "text-decoration: line-through;") => "text-decoration: underline line-through;",
        //     (var u, "") => u,
        //     ("", var st) => st,
        //     _ => ""
        // };

        // Scoped CSS (only widgets under a root carrying the .clingy-<id> class)
        return $@"
                .{scopeClass} #clingy-window {{
                    background: {style.BodyColor};
                }}
                .{scopeClass} #clingy-content,
                .{scopeClass} #clingy-content > viewport,
                .{scopeClass} #clingy-content-view,
                .{scopeClass} textview#clingy-content-view view {{
                    background: {style.BodyColor};
                    color: {style.BodyFontColor};
                    font-family: '{Escape(style.BodyFontName)}';
                    font-size: {style.BodyFontSize}px;
                    /*{bold}{italic}*/
                }}
                .{scopeClass} textview#clingy-content-view text {{
                    background: {style.BodyColor};
                    color: {style.BodyFontColor};
                    caret-color: {style.BodyFontColor};
                    font-family: '{Escape(style.BodyFontName)}';
                    font-size: {style.BodyFontSize}px;
                    /*{bold}{italic}{textDeco}*/
                }}";
    }

    private static string Escape(string s) => s.Replace("'", "\\'");
}
