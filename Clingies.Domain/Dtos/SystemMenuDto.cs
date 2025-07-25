using System;
using System.Diagnostics.CodeAnalysis;
using Clingies.Domain.Models;

namespace Clingies.Domain.Dtos;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "DB column mappings use snake_case")]
public class SystemMenuDto
{
    public string id { get; set; } = default!;
    public required string menu_type { get; set; }
    public string? parent_id { get; set; }
    public string? label { get; set; }
    public string? tooltip { get; set; }
    public string? icon { get; set; }
    public bool enabled { get; set; } = true;
    public bool separator { get; set; }
    public int sort_order { get; set; }

    // Populated later:
    public List<SystemMenuDto> children { get; } = new();

    public SystemMenu ToEntity()
    {
        return new SystemMenu
        {
            Id = id,
            MenuType = menu_type,
            ParentId = parent_id,
            Label = label,
            Tooltip = tooltip,
            Enabled = enabled,
            Separator = separator,
            SortOrder = sort_order
        };
    }
}
