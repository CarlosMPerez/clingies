using System;

namespace Clingies.App.Windows.CustomEventArgs;

public class TitleChangeRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }
    public string PreviousTitle { get; }

    public TitleChangeRequestedEventArgs(Guid id, string previousTitle)
    {
        ClingyId = id;
        PreviousTitle = previousTitle;
    }

}
