using System;
using Gtk;

namespace Clingies.GtkFront.Services;

public static class GtkTweaks
{
     private static bool _cssLoaded;
    public static void MakeEntryCompact(Entry entry, int pxHeight = 22, int fontPt = 10)
    {
        if (!_cssLoaded)
        {
            // Load once per app
            var css = @"
                /* class-based so we don't affect other entries */
                .compact-entry {
                    min-height: 0;          /* override theme */
                    padding-top: 0;         /* less vertical padding */
                    padding-bottom: 0;
                }";
            var provider = new CssProvider();
            provider.LoadFromData(css);
            StyleContext.AddProviderForScreen(Gdk.Screen.Default, provider, 800); // high priority
            _cssLoaded = true;
        }

        // 1) cut font size a bit (optional but effective)
        var fd = Pango.FontDescription.FromString($"Sans {fontPt}");
        entry.OverrideFont(fd);

        // 2) mark it with our compact class so the CSS applies
        entry.StyleContext.AddClass("compact-entry");

        // 3) ask for a small height; CSS ‘min-height: 0’ lets this take effect
        entry.HeightRequest = pxHeight;
    }
}
