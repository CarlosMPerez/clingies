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
}
