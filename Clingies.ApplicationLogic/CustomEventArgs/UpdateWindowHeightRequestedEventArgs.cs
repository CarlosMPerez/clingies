namespace Clingies.ApplicationLogic.CustomEventArgs;

public class UpdateWindowHeightRequestedEventArgs
{
    public Guid ClingyId { get; }

    public double Height { get; }

    public UpdateWindowHeightRequestedEventArgs(Guid id, double height)
    {
        ClingyId = id;
        Height = height;
    }
}
