using System;

namespace Clingies.ApplicationLogic.CustomEventArgs;

public class ContentChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }

    public string? Content { get; }

    public double Height { get; }

    public ContentChangeRequestedEventArgs(int id, string? content, double height)
    {
        ClingyId = id;
        Content = content;
        Height = height;
    }

}
