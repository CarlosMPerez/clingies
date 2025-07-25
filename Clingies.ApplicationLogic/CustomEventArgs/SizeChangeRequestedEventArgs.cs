using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class SizeChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }

    public double Width { get; }

    public double Height { get; }

    public SizeChangeRequestedEventArgs(int id, double width, double height)
    {
        ClingyId = id;
        Width = width;
        Height = height;
    }

}
