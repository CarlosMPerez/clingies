using Clingies.Application.Tests.Fakes;

namespace Clingies.Application.Tests;

public class ClingyServiceTests
{
    [Fact]
    public void GetAllActive_ReturnsRepositoryResults()
    {
        var repoItems = new List<ClingyModel> { new() { Id = 1 }, new() { Id = 2 } };
        var repo = new FakeClingyRepository { OnGetAllActive = () => repoItems };
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);

        var result = service.GetAllActive();

        Assert.Equal(repoItems.Select(x => x.Id), result.Select(x => x.Id));
        Assert.NotSame(repoItems, result);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Get_ReturnsRepositoryItem()
    {
        var expected = new ClingyModel { Id = 7, Title = "Hello" };
        var repo = new FakeClingyRepository { OnGet = id => expected };
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);

        var result = service.Get(7);

        Assert.Same(expected, result);
        Assert.Equal(7, repo.LastId);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Create_SetsCreatedAtAndReturnsRepositoryId()
    {
        var repo = new FakeClingyRepository { OnCreate = model => 42 };
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);
        var model = new ClingyModel { CreatedAt = DateTime.MinValue };
        var before = DateTime.UtcNow;

        var result = service.Create(model);

        var after = DateTime.UtcNow;
        Assert.Equal(42, result);
        Assert.Same(model, repo.LastModel);
        Assert.InRange(model.CreatedAt, before, after);
    }

    [Fact]
    public void Update_DelegatesToRepository()
    {
        var repo = new FakeClingyRepository();
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);
        var model = new ClingyModel { Id = 12 };

        service.Update(model);

        Assert.Same(model, repo.LastModel);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Close_DelegatesToRepository()
    {
        var repo = new FakeClingyRepository();
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);

        service.Close(3);

        Assert.Equal(3, repo.LastId);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void HardDelete_DelegatesToRepository()
    {
        var repo = new FakeClingyRepository();
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);

        service.HardDelete(13);

        Assert.Equal(13, repo.LastId);
        Assert.Empty(logger.ErrorEntries);
    }

    [Theory]
    [InlineData("GetAllActive")]
    [InlineData("Get")]
    [InlineData("Create")]
    [InlineData("Update")]
    [InlineData("Close")]
    [InlineData("HardDelete")]
    public void Methods_LogAndRethrow_OnRepositoryFailure(string methodName)
    {
        var expected = new InvalidOperationException(methodName);
        var repo = new FakeClingyRepository
        {
            OnGetAllActive = () => throw expected,
            OnGet = _ => throw expected,
            OnCreate = _ => throw expected,
            OnUpdate = _ => throw expected,
            OnClose = _ => throw expected,
            OnHardDelete = _ => throw expected
        };
        var logger = new TestLogger();
        var service = new ClingyService(repo, logger);

        Exception thrown = methodName switch
        {
            "GetAllActive" => Assert.Throws<InvalidOperationException>(() => service.GetAllActive()),
            "Get" => Assert.Throws<InvalidOperationException>(() => service.Get(1)),
            "Create" => Assert.Throws<InvalidOperationException>(() => service.Create(new ClingyModel())),
            "Update" => Assert.Throws<InvalidOperationException>(() => service.Update(new ClingyModel())),
            "Close" => Assert.Throws<InvalidOperationException>(() => service.Close(1)),
            "HardDelete" => Assert.Throws<InvalidOperationException>(() => service.HardDelete(1)),
            _ => throw new NotSupportedException(methodName)
        };

        Assert.Same(expected, thrown);
        var entry = Assert.Single(logger.ErrorEntries);
        Assert.Same(expected, entry.Exception);
    }
}
