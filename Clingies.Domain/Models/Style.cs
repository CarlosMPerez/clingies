using System;
using Clingies.Domain.Dtos;

namespace Clingies.Domain.Models;

public class Style
{
    public required string Id { get; set; }
    public required string BodyColor { get; set; }
    public required string TitleColor { get; set; }
    public required string BodyFont { get; set; }
    public required string BodyFontColor { get; set; }
    public required int BodyFontSize { get; set; }
    public required string BodyFontDecorations { get; set; }
    public required string TitleFont { get; set; }
    public required int TitleFontSize { get; set; }
    public required string TitleFontColor { get; set; }
    public required string TitleFontDecorations { get; set; }

    public StyleDto ToDto()
    {
        return new StyleDto
        {
            id = Id,
            body_color = BodyColor,
            title_color = TitleColor,
            body_font = BodyFont,
            body_font_color = BodyFontColor,
            body_font_size = BodyFontSize,
            body_font_decorations = BodyFontDecorations,
            title_font = TitleFont,
            title_font_size = TitleFontSize,
            title_font_color = TitleFontColor,
            title_font_decorations = TitleFontDecorations
        };
    }
}
