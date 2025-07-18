using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class RollRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }
    public bool IsRolled { get; }

    public RollRequestedEventArgs(Guid id, bool isRolled)
    {
        ClingyId = id;
        IsRolled = isRolled;
    }

}