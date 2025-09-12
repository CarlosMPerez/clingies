using Clingies.Domain.DTOs;

namespace Clingies.Domain.Models;

public sealed class Clingy
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public ClingyProperties Properties { get; set; }
    public ClingyContent Content { get; set; }
    public ClingyType Type { get; set; }

    public bool IsDeleted { get; set; }

    [IgnoreComparisonFieldAttribute]
    public DateTime CreatedAt { get; set; }

    public Clingy() { }


    public ClingyDto ToDto()
    {
        return new ClingyDto()
        {
            Id = Id,
            Title = Title,
            Type = Type,
            CreatedAt = CreatedAt,
            Text = Content.Text,
            PngBytes = Content.Png,
            PositionX = Properties.PositionX,
            PositionY = Properties.PositionY,
            Width = Properties.Width,
            Height = Properties.Height,
            IsPinned = Properties.IsPinned,
            IsLocked = Properties.IsLocked,
            IsRolled = Properties.IsRolled,
            IsStanding = Properties.IsStanding,
            IsDeleted = IsDeleted
        };
    }
}
