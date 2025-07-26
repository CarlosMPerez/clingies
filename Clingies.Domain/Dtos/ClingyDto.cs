
using System.Diagnostics.CodeAnalysis;
using Clingies.Domain.Models;

namespace Clingies.Domain.Dtos;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "DB column mappings use snake_case")]
public class ClingyDto
{
    public required int id { get; set; }
    public string content { get; set; } = string.Empty;
    public string? title { get; set; }
    public DateTime created_at { get; set; }
    public DateTime? modified_at { get; set; }
    public string? style { get; set; } = null;
    public bool is_deleted { get; set; }
    public bool is_pinned { get; set; }
    public bool is_rolled { get; set; }
    public bool is_locked { get; set; }

    public double position_x { get; set; }
    public double position_y { get; set; }

    public double width { get; set; }
    public double height { get; set; }

    public ClingyDto() { }

    public Clingy ToEntity()
    {
        return new Clingy
        {
            Id = id,
            Title = string.IsNullOrEmpty(title) ? "" : title,
            Content = content,
            PositionX = position_x,
            PositionY = position_y,
            Width = width,
            Height = height,
            Style = style,
            IsPinned = is_pinned,
            IsLocked = is_locked,
            IsRolled = is_rolled,
            IsDeleted = is_deleted,
            CreatedAt = created_at,
            ModifiedAt = modified_at
        };
    }    
}
