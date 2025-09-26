namespace Clingies.Application.Interfaces;

public interface IIconPathRepository
{
    public string? GetLightPath(string id);
    public string? GetDarkPath(string id);
}
