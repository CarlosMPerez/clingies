using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class RollRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }
    public bool IsRolled { get; }

    public RollRequestedEventArgs(int id, bool isRolled)
    {
        ClingyId = id;
        IsRolled = isRolled;
    }

}