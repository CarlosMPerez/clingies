using Clingies.Domain.Models;

namespace Clingies.Domain.DTOs;

public class ClingyDto
{
    // base and meta
    public int Id { get; set; }
    public ClingyType Type { get; set; }
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

    // content
    public string? Text { get; set; }
    public byte[]? PngBytes { get; set; }

    public ClingyDto()
    {
        // default values
        Id = 0;
        Title = "";
        Height = 100;
        Width = 300;
        CreatedAt = DateTime.UtcNow;
    }

    public Clingy ToEntity()
    {
        return new Clingy()
        {
            Id = Id,
            Title = Title,
            Type = Type,
            Content = new ClingyContent
            {
                Text = Text,
                Png = PngBytes
            },
            Properties = new ClingyProperties
            {
                Id = Id,
                PositionX = PositionX,
                PositionY = PositionY,
                Width = Width,
                Height = Height,
                IsPinned = IsPinned,
                IsLocked = IsLocked,
                IsRolled = IsRolled,
                IsStanding = IsStanding,
            },
            IsDeleted = IsDeleted
        };
    }
}
