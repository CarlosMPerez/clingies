namespace Clingies.ApplicationLogic.CustomEventArgs;

public class UpdateWindowHeightRequestedEventArgs
{
    public int ClingyId { get; }

    public double Height { get; }

    public UpdateWindowHeightRequestedEventArgs(int id, double height)
    {
        ClingyId = id;
        Height = height;
    }
}
