using System;
using System.Diagnostics.CodeAnalysis;
using Clingies.Domain.Models;

namespace Clingies.Domain.Dtos;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "DB column mappings use snake_case")]
public class StyleDto
{
    public required string id { get; set; }
    public required string body_color { get; set; }
    public required string title_color { get; set; }
    public required string body_font { get; set; }
    public required string body_font_color { get; set; }
    public required int body_font_size { get; set; }
    public required string body_font_decorations { get; set; }
    public required string title_font { get; set; }
    public required int title_font_size { get; set; }
    public required string title_font_color { get; set; }
    public required string title_font_decorations { get; set; }
    public required bool is_default { get; set; }
    public required bool is_active { get; set; }

    public Style ToEntity()
    {
        return new Style
        {
            Id = id,
            BodyColor = body_color,
            TitleColor = title_color,
            BodyFont = body_font,
            BodyFontColor = body_font_color,
            BodyFontSize = body_font_size,
            BodyFontDecorations = body_font_decorations,
            TitleFont = title_font,
            TitleFontSize = title_font_size,
            TitleFontColor = title_font_color,
            TitleFontDecorations = title_font_decorations,
            IsDefault = is_default,
            IsActive = is_active
        };
    }
}
