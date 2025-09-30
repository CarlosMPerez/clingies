using System;

namespace Clingies.Application.CustomEventArgs;

public class StyleChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }

    public int StyleId { get; }

    public StyleChangeRequestedEventArgs(int id, int styleId)
    {
        ClingyId = id;
        StyleId = styleId;
    }
}
