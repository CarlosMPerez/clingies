using System;

namespace Clingies.GtkFront.Windows.Parts;

public sealed class ClingyWindowCallbacks
{
    public int ClingyId { get; }

    public Action CloseRequested { get; }
    public Action<int, int> PositionChanged { get; }
    public Action<int, int> SizeChanged { get; }
    public Action<string> ContentChanged { get; }
    public Action<string> TitleChanged { get; }
    public Action<bool> PinChanged { get; }
    public Action<bool> RollChanged { get; }

    /// <summary>
    /// Class for calling WindowManager events from inside the window, or title or content
    /// </summary>
    /// <param name="clingyId"></param>
    /// <param name="closeRequested"></param>
    /// <param name="positionChanged"></param>
    /// <param name="sizeChanged"></param>
    /// <param name="titleChanged"></param>
    /// <param name="contentChanged"></param>
    /// <param name="pinChanged"></param>
    public ClingyWindowCallbacks(int clingyId,
                                Action closeRequested,
                                Action<int, int> positionChanged,
                                Action<int, int> sizeChanged,
                                Action<string> titleChanged,
                                Action<string> contentChanged,
                                Action<bool> pinChanged,
                                Action<bool> rollChanged)
    {
        ClingyId = clingyId;
        CloseRequested = closeRequested;
        PositionChanged = positionChanged;
        SizeChanged = sizeChanged;
        TitleChanged = titleChanged;
        ContentChanged = contentChanged;
        PinChanged = pinChanged;
        RollChanged = rollChanged;
    }
}
