using Clingies.Domain.DTOs;

namespace Clingies.Domain.Models;

public sealed class Clingy
{
    public int Id { get; set; }
    public Enums.ClingyType Type { get; set; }
    public string? Title { get; set; }
    public bool IsDeleted { get; set; }

    [IgnoreComparisonField]
    public DateTime CreatedAt { get; set; }
    [IgnoreComparisonField]
    public ClingyProperties Properties { get; set; }
    [IgnoreComparisonField]
    public ClingyContent Content { get; set; }

    public Clingy() { }

    public ClingyDto ToDto()
    {
        return new ClingyDto()
        {
            Id = Id,
            Type = Type,
            Title = Title,
            IsDeleted = IsDeleted,
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
            IsStanding = Properties.IsStanding
        };
    }
}
