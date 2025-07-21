namespace Clingies.ApplicationLogic.CustomEventArgs;

public class LockRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }
    public bool IsLocked { get; }

    public LockRequestedEventArgs(Guid id, bool isPinned)
    {
        ClingyId = id;
        IsLocked = isPinned;
    }

}
