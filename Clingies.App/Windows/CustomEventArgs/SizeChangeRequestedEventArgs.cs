using System;

namespace Clingies.App.Windows.CustomEventArgs;

public class SizeChangeRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }

    public double Width { get; }

    public double Height { get; }

    public SizeChangeRequestedEventArgs(Guid id, double width, double height)
    {
        ClingyId = id;
        Width = width;
        Height = height;
    }

}
