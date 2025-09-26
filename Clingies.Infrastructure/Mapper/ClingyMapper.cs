using Clingies.Domain.Models;
using Clingies.Infrastructure.Entities;

namespace Clingies.Infrastructure.Mapping;

public static class EntitiesMapper
{
    public static ClingyEntity ToEntity(this ClingyModel model) => new()
    {
        Id = model.Id,
        Type = model.Type,
        Title = model.Title,
        CreatedAt = model.CreatedAt,
        IsDeleted = model.IsDeleted,
        Content = new ClingyContentEntity
        {
            Id = model.Id,
            Text = model.Text,
            Png = model.PngBytes
        },
        Properties = new ClingyPropertiesEntity
        {
            Id = model.Id,
            PositionX = model.PositionX,
            PositionY = model.PositionY,
            Width = model.Width,
            Height = model.Height,
            IsPinned = model.IsPinned,
            IsLocked = model.IsLocked,
            IsRolled = model.IsRolled,
            IsStanding = model.IsStanding,
        }
    };

    public static ClingyModel ToModel(this ClingyEntity entity) => new()
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
}
