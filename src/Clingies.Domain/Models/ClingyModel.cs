namespace Clingies.Domain.Models;

public class ClingyModel
{
    public int Id { get; set; }
    public Enums.ClingyType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // properties
    public double PositionX { get; set; }
    public double PositionY { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsPinned { get; set; }
    public bool IsRolled { get; set; }
    public bool IsLocked { get; set; }
    public bool IsStanding { get; set; }
    public int StyleId { get; set; }

    // content
    public string? Text { get; set; }
    public byte[]? PngBytes { get; set; }

    public StyleModel Style { get; set; }

    public ClingyModel()
    {
        // default values
        Id = 0;
        Title = "";
        Height = AppConstants.Dimensions.DefaultClingyHeight;
        Width = AppConstants.Dimensions.DefaultClingyWidth;
        CreatedAt = DateTime.UtcNow;
        Style = new StyleModel();
    }
}
