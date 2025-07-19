namespace Clingies.Domain.Interfaces;

public interface IIconPathRepository
{
    public string? GetLightPath(string id);
    public string? GetDarkPath(string id);
}
