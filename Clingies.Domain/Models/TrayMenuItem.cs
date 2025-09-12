using Clingies.Domain.DTOs;

namespace Clingies.Domain.Models;

public class TrayMenuItem
{
    public string Id { get; set; } = default!;
    public string? Label { get; set; }
    public string? Tooltip { get; set; }
    public string? Icon { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Separator { get; set; }
    public string? ParentId { get; set; }
    public int SortOrder { get; set; }
    // Populated later:
    public List<TrayMenuItem> Children { get; } = new();

    public TrayMenuItemDto ToDto()
    {
        return new TrayMenuItemDto()
        {
            Id = this.Id,
            Label = this.Label,
            Tooltip = this.Tooltip,
            Icon = this.Icon,
            Enabled = this.Enabled,
            Separator = this.Separator,
            ParentId = this.ParentId,
            SortOrder = this.SortOrder
        };
    }
}
