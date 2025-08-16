using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class TitleChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }
    
    public string NewTitle { get; }

    public TitleChangeRequestedEventArgs(int id, string newTitle)
    {
        ClingyId = id;
        NewTitle = newTitle; 
    }

}
