
namespace Clingies.Application.Interfaces;

public interface ITitleDialogService
{
    string? ShowChangeTitleDialog(object? uiParent, string? initialTitle);
}
