using Clingies.Domain.Models;

namespace Clingies.Domain.Factories;

public static class ClingyFactory
{
    public static Clingy CreateNew(string title = "", string content = "")
    {
        return new Clingy(
            Guid.NewGuid(),
            title,
            content,
            x: 100,
            y: 100,
            width: 300,
            height: 100
        );
    }
}
