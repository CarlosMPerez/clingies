using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class SizeChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }

    public double Width { get; }

    public double Height { get; }

    public SizeChangeRequestedEventArgs(int id, double height)
    {
        ClingyId = id;
        Height = height;
    }

}
