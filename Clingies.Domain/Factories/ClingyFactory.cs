using Clingies.Domain.Models;

namespace Clingies.Domain.Factories;

public static class ClingyFactory
{
    public static Clingy CreateNew(string title = "", string content = "")
    {
        // Defaults
        return new Clingy(
            Guid.NewGuid(),
            title,
            content
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
            dto.IsPinned
        );
    }
}
