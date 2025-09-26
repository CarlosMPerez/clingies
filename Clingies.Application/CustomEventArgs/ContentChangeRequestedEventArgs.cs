
namespace Clingies.Application.CustomEventArgs;

public class ContentChangeRequestedEventArgs : EventArgs
{
    public int ClingyId { get; }

    public string? Content { get; }

    public double Height { get; }

    public ContentChangeRequestedEventArgs(int id, string? content)
    {
        ClingyId = id;
        Content = content;
    }

}
