using Clingies.Host.Windows;

namespace Clingies.Host.Services;

public sealed class TitleDialogService : ITitleDialogService
{
    public string? ShowChangeTitleDialog(object? uiParent, string? initialTitle)
    {
        var parent = uiParent as Gtk.Window;
        return TitleChangeDialog.Show(parent, initialTitle);
    }

}
