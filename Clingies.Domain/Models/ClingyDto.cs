
namespace Clingies.Domain.Models;

public class ClingyDto
{
    public required int Id { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public double PositionX { get; set; }
    public double PositionY { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsPinned { get; set; }
    public bool IsRolled { get; set; }
    public bool IsLocked { get; set; }
    public bool IsStanding { get; set; }
    public bool IsDeleted { get; set; }


    public ClingyDto() { }

    public Clingy ToEntity()
    {
        return new Clingy()
        {
            Id = Id,
            Title = Title,
            Content = Content,
            PositionX = PositionX,
            PositionY = PositionY,
            Width = Width,
            Height = Height,
            IsPinned = IsPinned,
            IsLocked = IsLocked,
            IsRolled = IsRolled,
            IsStanding = IsStanding,
            IsDeleted = IsDeleted
        };
    }
}
