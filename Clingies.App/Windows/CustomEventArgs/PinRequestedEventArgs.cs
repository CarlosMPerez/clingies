using System;

namespace Clingies.App.Windows.CustomEventArgs;

public class PinRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }
    public bool IsPinned { get; }

    public PinRequestedEventArgs(Guid id, bool isPinned)
    {
        ClingyId = id;
        IsPinned = isPinned;
    }

}
