namespace Clingies.Domain.Models;

public class ClingyModel
{
    public int Id { get; set; } = 0;
    public Enums.ClingyType Type { get; set; }
    public string Title { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ChangedAt { get; set; }

    // properties
    public double PositionX { get; set; }
    public double PositionY { get; set; }

    public double Width { get; set; } = AppConstants.Dimensions.DefaultClingyWidth;
    public double Height { get; set; } = AppConstants.Dimensions.DefaultClingyHeight;
    public bool IsPinned { get; set; }
    public bool IsRolled { get; set; }
    public bool IsLocked { get; set; }
    public bool IsStanding { get; set; }
    public int StyleId { get; set; }

    // content
    public string? Text { get; set; }
    public byte[]? PngBytes { get; set; }

    public StyleModel Style { get; set; } = new();
}
