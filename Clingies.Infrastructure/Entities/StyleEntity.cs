
namespace Clingies.Infrastructure.Entities;

public class StyleEntity
{
    public int Id { get; set; }
    public string StyleName { get; set; } = string.Empty;
    public string BodyColor { get; set; } = AppConstants.SystemStyle.BodyColor;
    public string BodyFontName { get; set; } = AppConstants.SystemStyle.BodyFontName;
    public string BodyFontColor { get; set; } = AppConstants.SystemStyle.BodyFontColor;
    public int BodyFontSize { get; set; } = AppConstants.SystemStyle.BodyFontSize;
    public Enums.FontDecorations BodyFontDecorations { get; set; } = AppConstants.SystemStyle.BodyFontDecorations;
    public bool IsSystem { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
