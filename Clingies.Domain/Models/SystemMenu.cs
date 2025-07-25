using System;
using Clingies.Domain.Dtos;

namespace Clingies.Domain.Models;

public class SystemMenu
{
    public string Id { get; set; } = default!;
    public required string MenuType { get; set; }
    public string? Label { get; set; }
    public string? Tooltip { get; set; }
    public string? Icon { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Separator { get; set; }
    public string? ParentId { get; set; }
    public int SortOrder { get; set; }

    // Populated later:
    public List<SystemMenu> Children { get; } = new();

    public SystemMenuDto ToDto()
    {
        return new SystemMenuDto
        {
            id = Id,
            menu_type = MenuType,
            parent_id = ParentId,
            label = Label,
            tooltip = Tooltip,
            enabled = Enabled,
            separator = Separator,
            sort_order = SortOrder
        };
    }
}
