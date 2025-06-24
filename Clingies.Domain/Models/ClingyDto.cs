
namespace Clingies.Domain.Models;

public class ClingyDto
{
    public required string Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsPinned { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public ClingyDto() { }
}
