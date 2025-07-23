using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class UpdateWindowSizeRequestedEventArgs
{
    public Guid ClingyId { get; }

    public double Height { get; }
    public double Width { get; }

    public UpdateWindowSizeRequestedEventArgs(Guid id, double width, double height)
    {
        ClingyId = id;
        Width = width;
        Height = height;
    }
}
