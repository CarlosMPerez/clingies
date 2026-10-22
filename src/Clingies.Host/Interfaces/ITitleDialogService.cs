
namespace Clingies.Host.Interfaces;

public interface ITitleDialogService
{
    string? ShowChangeTitleDialog(object? uiParent, string? initialTitle);
}
