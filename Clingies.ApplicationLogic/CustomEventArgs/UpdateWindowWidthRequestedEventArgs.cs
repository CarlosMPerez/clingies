using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class UpdateWindowWidthRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }
    public double Width { get; }

    public UpdateWindowWidthRequestedEventArgs(Guid id, double width)
    {
        ClingyId = id;
        Width = width;
    }

}
