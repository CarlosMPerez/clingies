namespace Clingies.ApplicationLogic.CustomEventArgs;

public class LockRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }
    public bool IsLocked { get; }

    public LockRequestedEventArgs(int id, bool isPinned)
    {
        ClingyId = id;
        IsLocked = isPinned;
    }

}
