namespace Clingies.Domain.Tests;

public class StyleModelTests
{
    [Fact]
    public void Constructor_UsesSystemStyleDefaults()
    {
        var style = new StyleModel();

        Assert.Equal(0, style.Id);
        Assert.Equal(string.Empty, style.StyleName);
        Assert.Equal(AppConstants.SystemStyle.BodyColor, style.BodyColor);
        Assert.Equal(AppConstants.SystemStyle.BodyFontName, style.BodyFontName);
        Assert.Equal(AppConstants.SystemStyle.BodyFontColor, style.BodyFontColor);
        Assert.Equal(AppConstants.SystemStyle.BodyFontSize, style.BodyFontSize);
        Assert.Equal((Enums.FontDecorations)AppConstants.SystemStyle.BodyFontDecorations, style.BodyFontDecorations);
        Assert.False(style.IsSystem);
        Assert.False(style.IsDefault);
        Assert.True(style.IsActive);
    }

    [Fact]
    public void Model_CanRepresentCustomStyleValues()
    {
        var style = new StyleModel
        {
            Id = 3,
            StyleName = "Custom",
            BodyColor = "#123456",
            BodyFontName = "Fira Code",
            BodyFontColor = "#FAFAFA",
            BodyFontSize = 18,
            BodyFontDecorations = Enums.FontDecorations.Bold | Enums.FontDecorations.Italic,
            IsSystem = true,
            IsDefault = true,
            IsActive = false
        };

        Assert.Equal(3, style.Id);
        Assert.Equal("Custom", style.StyleName);
        Assert.Equal("#123456", style.BodyColor);
        Assert.Equal("Fira Code", style.BodyFontName);
        Assert.Equal("#FAFAFA", style.BodyFontColor);
        Assert.Equal(18, style.BodyFontSize);
        Assert.Equal(Enums.FontDecorations.Bold | Enums.FontDecorations.Italic, style.BodyFontDecorations);
        Assert.True(style.IsSystem);
        Assert.True(style.IsDefault);
        Assert.False(style.IsActive);
    }
}
