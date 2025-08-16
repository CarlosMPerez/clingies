namespace Clingies.ApplicationLogic.CustomEventArgs;

public class PinRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }
    public bool IsPinned { get; }

    public PinRequestedEventArgs(int id, bool isPinned)
    {
        ClingyId = id;
        IsPinned = isPinned;
    }

}
