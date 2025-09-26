using Clingies.Infrastructure.Entities;
using Clingies.Domain.Models;

namespace Clingies.Infrastructure.Mapper;

public static class MenuMapper
{
    public static MenuItemModel ToModel(this MenuItemEntity entity) => new()
    {
        Id = entity.Id,
        Label = entity.Label,
        Tooltip = entity.Tooltip,
        Icon = entity.Icon,
        Enabled = entity.Enabled,
        Separator = entity.Separator,
        ParentId = entity.ParentId,
        SortOrder = entity.SortOrder
    };

    public static MenuItemEntity ToEntity(this MenuItemModel model) => new()
    {
        Id = model.Id,
        Label = model.Label,
        Tooltip = model.Tooltip,
        Icon = model.Icon,
        Enabled = model.Enabled,
        Separator = model.Separator,
        ParentId = model.ParentId,
        SortOrder = model.SortOrder
    };    
}
