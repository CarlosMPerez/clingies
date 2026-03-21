using Clingies.Domain.Models;
using Clingies.Infrastructure.Entities;

namespace Clingies.Infrastructure.Mapper;

public static class StyleMapper
{
    public static StyleEntity ToEntity(this StyleModel model) => new()
    {
        Id = model.Id,
        StyleName = model.StyleName,
        BodyColor = model.BodyColor,
        BodyFontName = model.BodyFontName,
        BodyFontColor = model.BodyFontColor,
        BodyFontSize = model.BodyFontSize,
        BodyFontDecorations = model.BodyFontDecorations,
        IsSystem = model.IsSystem,
        IsDefault = model.IsDefault,
        IsActive = model.IsActive
    };

    public static StyleModel ToModel(this StyleEntity entity) => new()
    {
        Id = entity.Id,
        StyleName = entity.StyleName,
        BodyColor = entity.BodyColor,
        BodyFontName = entity.BodyFontName,
        BodyFontColor = entity.BodyFontColor,
        BodyFontSize = entity.BodyFontSize,
        BodyFontDecorations = entity.BodyFontDecorations,
        IsSystem = entity.IsSystem,
        IsDefault = entity.IsDefault,
        IsActive = entity.IsActive
    };
}
