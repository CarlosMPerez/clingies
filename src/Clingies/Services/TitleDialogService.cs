using Clingies.Windows;

namespace Clingies.Services;

public sealed class TitleDialogService : ITitleDialogService
{
    public string? ShowChangeTitleDialog(object? uiParent, string? initialTitle)
    {
        var parent = uiParent as Gtk.Window;
        return TitleChangeDialog.Show(parent, initialTitle);
    }

}
