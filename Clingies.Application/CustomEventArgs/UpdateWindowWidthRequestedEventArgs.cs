using System;

namespace Clingies.Application.CustomEventArgs;

public class UpdateWindowWidthRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }
    public double Width { get; }

    public UpdateWindowWidthRequestedEventArgs(int id, double width)
    {
        ClingyId = id;
        Width = width;
    }

}
