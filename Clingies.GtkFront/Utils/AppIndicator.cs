using System;
using Gtk;
using appIndiNative = Clingies.GtkFront.Utils.AppIndicatorNative;

namespace Clingies.GtkFront.Utils;

public sealed class AppIndicator : IDisposable
{
    private IntPtr _handle;
    public AppIndicator(string id, string iconName, appIndiNative.AppIndicatorCategory category)
    {
        _handle = appIndiNative.app_indicator_new(id, iconName, category);
        if (_handle == IntPtr.Zero)
            throw new DllNotFoundException("Failed to create AppIndicator. Is libayatana-appindicator3 installed?");
    }

    public void SetStatus(appIndiNative.AppIndicatorStatus status) => appIndiNative.app_indicator_set_status(_handle, status);
    public void SetIcon(string iconName, string? desc = null) => appIndiNative.app_indicator_set_icon_full(_handle, iconName, desc ?? iconName);
    public void SetTitle(string title) => appIndiNative.app_indicator_set_title(_handle, title);
    public void SetMenu(Menu menu) => appIndiNative.SetMenu(_handle, menu);

    public void Dispose()
    {
        // libayatana-appindicator doesn't expose a destroy; let the process cleanup handle it.
        _handle = IntPtr.Zero;
    }
}
