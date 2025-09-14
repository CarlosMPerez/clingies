namespace Clingies.Utils;

public class Enums
{

    public enum AppIndicatorCategory
    {
        ApplicationStatus = 0,
        Communications = 1,
        SystemServices = 2,
        Hardware = 3,
        Other = 4
    }

    public enum AppIndicatorStatus
    {
        Passive = 0,
        Active = 1,
        Attention = 2
    }

    public enum ClingyType
    {
        Unknown = 0,
        Desktop = 1,
        Sleeping = 2,
        Recurring = 3,
        Closed = 4,
        Stored = 5
    }


}
