using System;

namespace Clingies.ApplicationLogic.Interfaces;

public interface ITitleDialogService
{
    string? ShowChangeTitleDialog(object? uiParent, string? initialTitle);
}
