using Clingies.Domain.Models;

namespace Clingies.Domain.DTOs;

public class TrayMenuItemDto
{
    public string Id { get; set; } = default!;
    public string? Label { get; set; }
    public string? Tooltip { get; set; }
    public string? Icon { get; set; }
    public bool Enabled { get; set; } = true;
    public bool Separator { get; set; }
    public string? ParentId { get; set; }
    public int SortOrder { get; set; }
    public TrayMenuItem ToEntity()
    {
        return new TrayMenuItem()
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
