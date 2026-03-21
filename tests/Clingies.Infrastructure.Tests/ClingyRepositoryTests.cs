using Clingies.Domain.Common;
using Clingies.Domain.Models;

namespace Clingies.Infrastructure.Tests;

public class ClingyRepositoryTests
{
    [Fact]
    public void Create_CreatesAggregateRowsAndNullContent()
    {
        using var db = new TestDatabase();

        var clingy = new ClingyModel
        {
            Title = "New clingy",
            Type = Enums.ClingyType.Desktop,
            StyleId = db.SystemStyleId,
            Text = "ignored on create",
            PngBytes = [1, 2, 3]
        };

        var id = db.Clingies.Create(clingy);
        var created = db.Clingies.Get(id);

        Assert.NotNull(created);
        Assert.Equal(id, created!.Id);
        Assert.Null(created.Text);
        Assert.Null(created.PngBytes);
        Assert.Equal(1, db.Scalar<int>("SELECT COUNT(*) FROM clingies WHERE id = @Id", new { Id = id }));
        Assert.Equal(1, db.Scalar<int>("SELECT COUNT(*) FROM clingy_properties WHERE id = @Id", new { Id = id }));
        Assert.Equal(1, db.Scalar<int>("SELECT COUNT(*) FROM clingy_content WHERE id = @Id", new { Id = id }));
    }

    [Fact]
    public void GetAllActive_ExcludesSoftDeletedClingies()
    {
        using var db = new TestDatabase();

        var activeId = db.CreateClingy(title: "Active");
        var deletedId = db.CreateClingy(title: "Deleted");

        db.Clingies.SoftDelete(deletedId);

        var active = db.Clingies.GetAllActive();

        Assert.Contains(active, x => x.Id == activeId);
        Assert.DoesNotContain(active, x => x.Id == deletedId);
    }

    [Fact]
    public void Update_PersistsTextOnlyAndClearsExistingImage()
    {
        using var db = new TestDatabase();

        var clingyId = db.CreateClingy();
        var clingy = db.Clingies.Get(clingyId)!;
        clingy.PngBytes = [9, 8, 7];
        db.Clingies.Update(clingy);

        clingy = db.Clingies.Get(clingyId)!;
        clingy.Text = "Hello";
        clingy.PngBytes = null;
        db.Clingies.Update(clingy);

        var updated = db.Clingies.Get(clingyId)!;

        Assert.Equal("Hello", updated.Text);
        Assert.Null(updated.PngBytes);
    }

    [Fact]
    public void Update_PersistsImageOnlyAndClearsExistingText()
    {
        using var db = new TestDatabase();

        var clingyId = db.CreateClingy();
        var clingy = db.Clingies.Get(clingyId)!;
        clingy.Text = "Text before image";
        db.Clingies.Update(clingy);

        clingy = db.Clingies.Get(clingyId)!;
        clingy.Text = null;
        clingy.PngBytes = [1, 3, 5, 7];
        db.Clingies.Update(clingy);

        var updated = db.Clingies.Get(clingyId)!;

        Assert.Null(updated.Text);
        Assert.Equal([1, 3, 5, 7], updated.PngBytes);
    }

    [Fact]
    public void Update_NormalizesWhitespaceTextAndEmptyPngToNull()
    {
        using var db = new TestDatabase();

        var clingyId = db.CreateClingy();
        var clingy = db.Clingies.Get(clingyId)!;
        clingy.Text = "   ";
        clingy.PngBytes = [];

        db.Clingies.Update(clingy);

        var updated = db.Clingies.Get(clingyId)!;

        Assert.Null(updated.Text);
        Assert.Null(updated.PngBytes);
    }

    [Fact]
    public void Update_RejectsTextAndImageAtTheSameTime()
    {
        using var db = new TestDatabase();

        var clingy = db.Clingies.Get(db.CreateClingy())!;
        clingy.Text = "Hello";
        clingy.PngBytes = [1];

        var ex = Assert.Throws<InvalidOperationException>(() => db.Clingies.Update(clingy));

        Assert.Contains("either Text or Png", ex.Message);
    }

    [Fact]
    public void SoftDeleteAndUnDelete_ToggleFlagsAndTypes()
    {
        using var db = new TestDatabase();

        var clingyId = db.CreateClingy();

        db.Clingies.SoftDelete(clingyId);

        var deletedState = db.QuerySingle<ClingyState>(
            "SELECT is_deleted AS IsDeleted, type_id AS TypeId FROM clingies WHERE id = @Id",
            new { Id = clingyId });

        Assert.True(deletedState.IsDeleted);
        Assert.Equal((int)Enums.ClingyType.Closed, deletedState.TypeId);
        Assert.DoesNotContain(db.Clingies.GetAllActive(), x => x.Id == clingyId);

        db.Clingies.UnDelete(clingyId);

        var restoredState = db.QuerySingle<ClingyState>(
            "SELECT is_deleted AS IsDeleted, type_id AS TypeId FROM clingies WHERE id = @Id",
            new { Id = clingyId });

        Assert.False(restoredState.IsDeleted);
        Assert.Equal((int)Enums.ClingyType.Desktop, restoredState.TypeId);
        Assert.Contains(db.Clingies.GetAllActive(), x => x.Id == clingyId);
    }

    [Fact]
    public void HardDelete_RemovesAggregateRows()
    {
        using var db = new TestDatabase();

        var clingyId = db.CreateClingy();

        db.Clingies.HardDelete(clingyId);

        Assert.Equal(0, db.Scalar<int>("SELECT COUNT(*) FROM clingies WHERE id = @Id", new { Id = clingyId }));
        Assert.Equal(0, db.Scalar<int>("SELECT COUNT(*) FROM clingy_properties WHERE id = @Id", new { Id = clingyId }));
        Assert.Equal(0, db.Scalar<int>("SELECT COUNT(*) FROM clingy_content WHERE id = @Id", new { Id = clingyId }));
    }

    [Fact]
    public void Update_MissingClingy_ThrowsKeyNotFound()
    {
        using var db = new TestDatabase();

        var missing = new ClingyModel
        {
            Id = 999_999,
            Title = "Missing",
            Type = Enums.ClingyType.Desktop,
            StyleId = db.SystemStyleId
        };

        Assert.Throws<KeyNotFoundException>(() => db.Clingies.Update(missing));
    }

    private sealed class ClingyState
    {
        public bool IsDeleted { get; init; }
        public int TypeId { get; init; }
    }
}
