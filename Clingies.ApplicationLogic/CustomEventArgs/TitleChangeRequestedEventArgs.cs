using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class TitleChangeRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }
    
    public string NewTitle { get; }

    public TitleChangeRequestedEventArgs(Guid id, string newTitle)
    {
        ClingyId = id;
        NewTitle = newTitle; 
    }

}
