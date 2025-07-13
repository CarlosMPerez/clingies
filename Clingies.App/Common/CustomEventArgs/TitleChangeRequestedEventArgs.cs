using System;

namespace Clingies.App.Common.CustomEventArgs;

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
