using System;

namespace Clingies.GtkFront.Windows.Parts;

internal sealed class ClingyWindowCallbacks
{
    public int ClingyId { get; }

    public Action CloseRequested { get; }
    public Action<int, int> PositionChanged { get; }
    public Action<int, int> SizeChanged { get; }
    public Action<string> ContentChanged { get; }
    public Action<string> TitleChanged { get; }
    public Action<bool> PinChanged { get; }
    public Action<bool> LockChanged { get; }

    public ClingyWindowCallbacks(int clingyId,
                                Action closeRequested,
                                Action<int, int> positionChanged,
                                Action<int, int> sizeChanged,
                                Action<string> titleChanged,
                                Action<string> contentChanged,
                                Action<bool> pinChanged,
                                Action<bool> lockChanged)
    {
        ClingyId = clingyId;
        CloseRequested = closeRequested;
        PositionChanged = positionChanged;
        SizeChanged = sizeChanged;
        TitleChanged = titleChanged;
        ContentChanged = contentChanged;
        PinChanged = pinChanged;
        LockChanged = lockChanged;
    }
}
