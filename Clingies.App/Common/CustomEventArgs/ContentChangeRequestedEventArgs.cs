using System;

namespace Clingies.App.Common.CustomEventArgs;

public class ContentChangeRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }

    public string? Content { get; }

    public double Height { get; }

    public ContentChangeRequestedEventArgs(Guid id, string? content, double height)
    {
        ClingyId = id;
        Content = content;
        Height = height;
    }

}
