using System;
using System.Runtime.InteropServices;
using Gtk;

namespace Clingies.GtkFront.Utils;

public static class AppIndicatorNative
{
    // The shared object from libayatana-appindicator3-1
    private const string Lib = "ayatana-appindicator3";

    public enum AppIndicatorCategory
    {
        ApplicationStatus = 0,
        Communications = 1,
        SystemServices = 2,
        Hardware = 3,
        Other = 4
    }

    public enum AppIndicatorStatus
    {
        Passive = 0,
        Active = 1,
        Attention = 2
    }

    // typedef struct _AppIndicator AppIndicator;
    // AppIndicator* app_indicator_new(const gchar* id, const gchar* icon_name, AppIndicatorCategory category);
    [DllImport(Lib)]
    public static extern IntPtr app_indicator_new(
        [MarshalAs(UnmanagedType.LPStr)] string id,
        [MarshalAs(UnmanagedType.LPStr)] string icon_name,
        AppIndicatorCategory category);

    // void app_indicator_set_status(AppIndicator* self, AppIndicatorStatus status);
    [DllImport(Lib)]
    public static extern void app_indicator_set_status(IntPtr self, AppIndicatorStatus status);

    // void app_indicator_set_menu(AppIndicator* self, GtkMenu* menu);
    [DllImport(Lib)]
    public static extern void app_indicator_set_menu(IntPtr self, IntPtr gtkMenu);

    // void app_indicator_set_icon_full(AppIndicator* self, const gchar* icon_name, const gchar* desc);
    [DllImport(Lib)]
    public static extern void app_indicator_set_icon_full(
        IntPtr self,
        [MarshalAs(UnmanagedType.LPStr)] string icon_name,
        [MarshalAs(UnmanagedType.LPStr)] string desc);

    // Optional: title shown in some panels (not required)
    [DllImport(Lib)]
    public static extern void app_indicator_set_title(
        IntPtr self,
        [MarshalAs(UnmanagedType.LPStr)] string title);

    public static void SetMenu(IntPtr indicator, Menu menu)
        => app_indicator_set_menu(indicator, menu?.Handle ?? IntPtr.Zero);
}
