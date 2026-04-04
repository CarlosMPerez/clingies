using Clingies.Domain.Models;
using Clingies.Infrastructure.CustomExceptions;

namespace Clingies.Infrastructure.Tests;

public class StyleRepositoryTests
{
    [Fact]
    public void Create_PersistsStyleAndMarksItAsDefault_WhenRequested()
    {
        using var db = new TestDatabase();

        var styleId = db.CreateStyle(isDefault: true);

        var created = db.Styles.Get(styleId);
        var defaultStyle = db.Styles.GetDefault();
        var defaults = db.Styles.GetAll().Where(x => x.IsDefault).ToList();

        Assert.NotNull(created);
        Assert.NotNull(defaultStyle);
        Assert.True(created.IsDefault);
        Assert.Equal(styleId, defaultStyle.Id);
        Assert.Single(defaults);
    }

    [Fact]
    public void Create_RejectsReservedSystemName()
    {
        using var db = new TestDatabase();

        var style = new StyleModel
        {
            StyleName = AppConstants.SystemStyle.Name
        };

        Assert.Throws<ReservedStyleNameException>(() => db.Styles.Create(style));
    }

    [Fact]
    public void Create_RejectsEleventhActiveStyle()
    {
        using var db = new TestDatabase();

        for (var i = 0; i < 9; i++)
            db.CreateStyle(name: $"Style-{i}", isActive: true);

        var ex = Assert.Throws<TooManyActiveStylesException>(() =>
            db.CreateStyle(name: "Style-over-limit", isActive: true));

        Assert.Contains("Active style", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarkActive_RejectsDisablingLastActiveStyle()
    {
        using var db = new TestDatabase();

        var ex = Assert.Throws<AtLeastOneActiveStyleException>(() =>
            db.Styles.MarkActive(db.SystemStyleId, false));

        Assert.Contains("at least one active style", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Delete_RejectsSystemStyle()
    {
        using var db = new TestDatabase();

        var ex = Assert.Throws<CannotDeleteSystemStyleException>(() =>
            db.Styles.Delete(db.SystemStyleId));

        Assert.Contains("cannot be deleted", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Delete_RejectsStyleInUseByActiveClingy()
    {
        using var db = new TestDatabase();

        var styleId = db.CreateStyle(name: "In-use-style");
        _ = db.CreateClingy(styleId, "Uses custom style");

        var ex = Assert.Throws<CannotDeleteStyleInUse>(() => db.Styles.Delete(styleId));

        Assert.Contains("being used", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Delete_AllowsStyleUsedOnlyByClosedClingy()
    {
        using var db = new TestDatabase();

        var styleId = db.CreateStyle(name: "Closed-only-style");
        var clingyId = db.CreateClingy(styleId, "Closed clingy");
        db.Clingies.Close(clingyId);

        db.Styles.Delete(styleId);

        Assert.Null(db.Styles.Get(styleId));
    }

    [Fact]
    public void Delete_RemovesUnusedDefaultStyleAndFallsBackToSystemDefault()
    {
        using var db = new TestDatabase();

        var styleId = db.CreateStyle(name: "Temporary default", isDefault: true);

        db.Styles.Delete(styleId);

        Assert.Null(db.Styles.Get(styleId));
        Assert.Equal(db.SystemStyleId, db.Styles.GetDefault()!.Id);
        Assert.Single(db.Styles.GetAll(), x => x.IsDefault);
    }
}
