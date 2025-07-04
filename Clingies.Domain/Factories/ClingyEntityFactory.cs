using Clingies.Domain.Models;

namespace Clingies.Domain.Factories;

public static class ClingyEntityFactory
{
    public static Clingy CreateNew(string title = "", string content = "", double posX = 0, double posY = 0)
    {
        // Defaults
        return new Clingy(
            Guid.NewGuid(),
            title,
            content,
            posX,
            posY
        );
    }

    public static Clingy FromDto(ClingyDto dto)
    {
        return new Clingy(
            Guid.Parse(dto.Id),
            dto.Title,
            dto.Content,
            dto.PositionX,
            dto.PositionY,
            dto.Width,
            dto.Height,
            dto.IsDeleted,
            dto.IsPinned,
            dto.IsRolled,
            dto.IsStand
        );
    }
}
