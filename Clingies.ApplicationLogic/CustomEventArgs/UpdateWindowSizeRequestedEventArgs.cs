using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class UpdateWindowSizeRequestedEventArgs
{
    public int ClingyId { get; }

    public double Height { get; }
    public double Width { get; }

    public UpdateWindowSizeRequestedEventArgs(int id, double width, double height)
    {
        ClingyId = id;
        Width = width;
        Height = height;
    }
}
