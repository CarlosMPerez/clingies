namespace Clingies.Application.CustomEventArgs;

public class LockRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }
    public bool IsLocked { get; }

    public LockRequestedEventArgs(int id, bool isLocked)
    {
        ClingyId = id;
        IsLocked = isLocked;
    }

}
