using Clingies.Domain.DTOs;
using Clingies.Infrastructure.Models;

namespace Clingies.Infrastructure.Mapping;

public static class ClingiesMapper
{
    public static Clingy ToEntity(this ClingyDto dto) => new()
    {
        Id = dto.Id,
        Type = dto.Type,
        Title = dto.Title,
        CreatedAt = dto.CreatedAt,
        IsDeleted = dto.IsDeleted,
        Content = new ClingyContent
        {
            Id = dto.Id,
            Text = dto.Text,
            Png = dto.PngBytes
        },
        Properties = new ClingyProperties
        {
            Id = dto.Id,
            PositionX = dto.PositionX,
            PositionY = dto.PositionY,
            Width = dto.Width,
            Height = dto.Height,
            IsPinned = dto.IsPinned,
            IsLocked = dto.IsLocked,
            IsRolled = dto.IsRolled,
            IsStanding = dto.IsStanding,
        }
    };

    public static ClingyDto ToDto(this Clingy entity) => new()
    {
        Id = entity.Id,
        Type = entity.Type,
        Title = entity.Title ?? string.Empty,
        IsDeleted = entity.IsDeleted,
        CreatedAt = entity.CreatedAt,
        Text = entity.Content.Text,
        PngBytes = entity.Content.Png,
        PositionX = entity.Properties.PositionX,
        PositionY = entity.Properties.PositionY,
        Width = entity.Properties.Width,
        Height = entity.Properties.Height,
        IsPinned = entity.Properties.IsPinned,
        IsLocked = entity.Properties.IsLocked,
        IsRolled = entity.Properties.IsRolled,
        IsStanding = entity.Properties.IsStanding
    };

    public static MenuItem ToEntity(this MenuItemDto dto) => new()
    {
        Id = dto.Id,
        Label = dto.Label,
        Tooltip = dto.Tooltip,
        Icon = dto.Icon,
        Enabled = dto.Enabled,
        Separator = dto.Separator,
        ParentId = dto.ParentId,
        SortOrder = dto.SortOrder
    };


    public static MenuItemDto ToDto(this MenuItem entity) => new()
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

}
