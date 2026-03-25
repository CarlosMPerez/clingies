using System;

namespace Clingies.Windows.Parts;

/// <summary>
/// Class for calling WindowManager events from inside the window, or title or content
/// </summary>
public sealed class ClingyWindowCallbacks(int clingyId,
                            Action closeRequested,
                            Action<int, int> positionChanged,
                            Action<int, int> sizeChanged,
                            Action<string> titleChanged,
                            Action<string> contentChanged,
                            Action<bool> pinChanged,
                            Action<bool> rollChanged)
{
    public int ClingyId { get; } = clingyId;

    public Action CloseRequested { get; } = closeRequested;
    public Action<int, int> PositionChanged { get; } = positionChanged;
    public Action<int, int> SizeChanged { get; } = sizeChanged;
    public Action<string> ContentChanged { get; } = contentChanged;
    public Action<string> TitleChanged { get; } = titleChanged;
    public Action<bool> PinChanged { get; } = pinChanged;
    public Action<bool> RollChanged { get; } = rollChanged;
}
