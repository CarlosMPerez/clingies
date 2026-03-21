namespace Clingies.Domain.Tests;

public class ClingyModelTests
{
    [Fact]
    public void Constructor_SetsExpectedDefaults()
    {
        var before = DateTime.UtcNow;
        var model = new ClingyModel();
        var after = DateTime.UtcNow;

        Assert.Equal(0, model.Id);
        Assert.Equal(string.Empty, model.Title);
        Assert.Equal(AppConstants.Dimensions.DefaultClingyWidth, model.Width);
        Assert.Equal(AppConstants.Dimensions.DefaultClingyHeight, model.Height);
        Assert.False(model.IsDeleted);
        Assert.False(model.IsPinned);
        Assert.False(model.IsRolled);
        Assert.False(model.IsLocked);
        Assert.False(model.IsStanding);
        Assert.Equal(0, model.PositionX);
        Assert.Equal(0, model.PositionY);
        Assert.Equal(0, model.StyleId);
        Assert.Null(model.Text);
        Assert.Null(model.PngBytes);
        Assert.NotNull(model.Style);
        Assert.InRange(model.CreatedAt, before, after);
    }

    [Fact]
    public void Constructor_CreatesIndependentStyleInstance()
    {
        var first = new ClingyModel();
        var second = new ClingyModel();

        first.Style.StyleName = "Changed";

        Assert.NotSame(first.Style, second.Style);
        Assert.Equal(string.Empty, second.Style.StyleName);
    }

    [Fact]
    public void Model_CanStoreDesktopStateAndContent()
    {
        var model = new ClingyModel
        {
            Id = 10,
            Type = Enums.ClingyType.Desktop,
            Title = "Note",
            PositionX = 50,
            PositionY = 75,
            Width = 400,
            Height = 120,
            IsPinned = true,
            IsRolled = true,
            IsLocked = true,
            IsStanding = true,
            StyleId = 7,
            Text = "Hello",
            PngBytes = [1, 2, 3]
        };

        Assert.Equal(10, model.Id);
        Assert.Equal(Enums.ClingyType.Desktop, model.Type);
        Assert.Equal("Note", model.Title);
        Assert.Equal(50, model.PositionX);
        Assert.Equal(75, model.PositionY);
        Assert.Equal(400, model.Width);
        Assert.Equal(120, model.Height);
        Assert.True(model.IsPinned);
        Assert.True(model.IsRolled);
        Assert.True(model.IsLocked);
        Assert.True(model.IsStanding);
        Assert.Equal(7, model.StyleId);
        Assert.Equal("Hello", model.Text);
        Assert.Equal([1, 2, 3], model.PngBytes);
    }
}
