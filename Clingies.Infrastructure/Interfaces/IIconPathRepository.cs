namespace Clingies.Infrastructure.Interfaces;

public interface IIconPathRepository
{
    public string? GetLightPath(string id);
    public string? GetDarkPath(string id);
}
