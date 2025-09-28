
namespace Clingies.Infrastructure.Entities;

public sealed class ClingyPropertiesEntity
{
    public int Id { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; } = AppConstants.Dimensions.DefaultClingyWidth;
    public double Height { get; set; } = AppConstants.Dimensions.DefaultClingyHeight;
    public bool IsPinned { get; set; }
    public bool IsRolled { get; set; }
    public bool IsLocked { get; set; }
    public bool IsStanding { get; set; }

    public int StyleId { get; set; }

    public StyleEntity Style { get; set; }

    public ClingyPropertiesEntity() => Style = new StyleEntity();
}
