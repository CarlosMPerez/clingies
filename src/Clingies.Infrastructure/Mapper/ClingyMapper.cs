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
            StyleId = model.StyleId,
            Style = new StyleEntity()
            {
                Id = model.StyleId,
                StyleName = model.Style.StyleName,
                BodyColor = model.Style.BodyColor,
                BodyFontName = model.Style.BodyFontName,
                BodyFontColor = model.Style.BodyFontColor,
                BodyFontSize = model.Style.BodyFontSize,
                BodyFontDecorations = model.Style.BodyFontDecorations,
                IsDefault = model.Style.IsDefault,
                IsActive = model.Style.IsActive
            }
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
        IsStanding = entity.Properties.IsStanding,
        StyleId = entity.Properties.StyleId,
        Style = new StyleModel()
        {
            Id = entity.Properties.Style.Id,
            StyleName = entity.Properties.Style.StyleName,
            BodyColor = entity.Properties.Style.BodyColor,
            BodyFontName = entity.Properties.Style.BodyFontName,
            BodyFontColor = entity.Properties.Style.BodyFontColor,
            BodyFontSize = entity.Properties.Style.BodyFontSize,
            BodyFontDecorations = entity.Properties.Style.BodyFontDecorations,
            IsDefault = entity.Properties.Style.IsDefault,
            IsActive = entity.Properties.Style.IsActive
        }
    };
}
