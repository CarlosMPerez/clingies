using System;

namespace Clingies.App.Common.CustomEventArgs;

public class ContentChangeRequestedEventArgs : EventArgs
{
    public Guid ClingyId { get; }

    public string Content { get; }

    public ContentChangeRequestedEventArgs(Guid id, string content)
    {
        ClingyId = id;
        Content = content;
    }

}
