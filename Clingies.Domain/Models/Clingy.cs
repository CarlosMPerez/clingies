using Clingies.Domain.Dtos;

namespace Clingies.Domain.Models;

public class Clingy
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public string Style { get; set; } = "default";
    public bool IsDeleted { get; set; } = false;
    public bool IsPinned { get; set; } = false;
    public bool IsRolled { get; set; } = false;
    public bool IsLocked { get; set; } = false;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Width { get; set; } = 300;
    public double Height { get; set; } = 100;

    public Clingy() { }

    public ClingyDto ToDto()
    {
        return new ClingyDto
        {
            id = Id,
            title = Title,
            content = Content,
            position_x = PositionX,
            position_y = PositionY,
            width = Width,
            height = Height,
            style = Style,
            is_pinned = IsPinned,
            is_locked = IsLocked,
            is_rolled = IsRolled,
            is_deleted = IsDeleted,
            created_at = CreatedAt,
            modified_at = ModifiedAt
        };
    }
}
