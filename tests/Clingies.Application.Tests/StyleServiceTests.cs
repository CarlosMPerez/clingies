using Clingies.Application.Tests.Fakes;

namespace Clingies.Application.Tests;

public class StyleServiceTests
{
    [Fact]
    public void GetAll_ReturnsRepositoryResults()
    {
        var repoItems = new List<StyleModel> { new() { Id = 1 }, new() { Id = 2 } };
        var repo = new FakeStyleRepository { OnGetAll = () => repoItems };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var result = service.GetAll();

        Assert.Same(repoItems, result);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void GetAllActive_ReturnsRepositoryResults()
    {
        var repoItems = new List<StyleModel> { new() { Id = 4 } };
        var repo = new FakeStyleRepository { OnGetAllActive = () => repoItems };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var result = service.GetAllActive();

        Assert.Same(repoItems, result);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Get_ReturnsRepositoryItem()
    {
        var expected = new StyleModel { Id = 11, StyleName = "Blue" };
        var repo = new FakeStyleRepository { OnGet = id => expected };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var result = service.Get(11);

        Assert.Same(expected, result);
        Assert.Equal(11, repo.LastId);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Get_WhenRepositoryReturnsNull_LogsAndThrowsKeyNotFound()
    {
        var repo = new FakeStyleRepository { OnGet = _ => null };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var ex = Assert.Throws<KeyNotFoundException>(() => service.Get(99));

        Assert.NotNull(ex);
        var entry = Assert.Single(logger.ErrorEntries);
        Assert.IsType<KeyNotFoundException>(entry.Exception);
        Assert.Contains("style 99", entry.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetDefault_ReturnsRepositoryStyle()
    {
        var expected = new StyleModel { Id = 3, StyleName = "Default" };
        var repo = new FakeStyleRepository { OnGetDefault = () => expected };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var result = service.GetDefault();

        Assert.Same(expected, result);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void GetSystemStyleId_ReturnsRepositoryValue()
    {
        var repo = new FakeStyleRepository { OnGetSystemStyleId = () => 77 };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var result = service.GetSystemStyleId();

        Assert.Equal(77, result);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Create_DelegatesToRepository()
    {
        var style = new StyleModel { Id = 1, StyleName = "New" };
        var repo = new FakeStyleRepository();
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        service.Create(style);

        Assert.Same(style, repo.LastStyle);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Update_DelegatesToRepository()
    {
        var style = new StyleModel { Id = 2, StyleName = "Updated" };
        var repo = new FakeStyleRepository();
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        service.Update(style);

        Assert.Same(style, repo.LastStyle);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Delete_RethrowsCustomExceptionWithoutLogging()
    {
        var expected = new DummyCustomException("custom");
        var repo = new FakeStyleRepository { OnDelete = _ => throw expected };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var thrown = Assert.Throws<DummyCustomException>(() => service.Delete(6));

        Assert.Same(expected, thrown);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void Delete_LogsAndRethrowsNonCustomException()
    {
        var expected = new InvalidOperationException("boom");
        var repo = new FakeStyleRepository { OnDelete = _ => throw expected };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        var thrown = Assert.Throws<InvalidOperationException>(() => service.Delete(6));

        Assert.Same(expected, thrown);
        var entry = Assert.Single(logger.ErrorEntries);
        Assert.Same(expected, entry.Exception);
        Assert.Contains("style 6", entry.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MarkActive_DelegatesToRepository()
    {
        var repo = new FakeStyleRepository();
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        service.MarkActive(10, true);

        Assert.Equal(10, repo.LastId);
        Assert.True(repo.LastBool);
        Assert.Empty(logger.ErrorEntries);
    }

    [Fact]
    public void MarkDefault_DelegatesToRepository()
    {
        var repo = new FakeStyleRepository();
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        service.MarkDefault(20, false);

        Assert.Equal(20, repo.LastId);
        Assert.False(repo.LastBool);
        Assert.Empty(logger.ErrorEntries);
    }

    [Theory]
    [InlineData("GetAllActive")]
    [InlineData("GetAll")]
    [InlineData("Get")]
    [InlineData("GetDefault")]
    [InlineData("GetSystemStyleId")]
    [InlineData("Create")]
    [InlineData("Update")]
    [InlineData("MarkActive")]
    [InlineData("MarkDefault")]
    public void Methods_LogAndRethrow_OnRepositoryFailure(string methodName)
    {
        var expected = new InvalidOperationException(methodName);
        var repo = new FakeStyleRepository
        {
            OnGetAllActive = () => throw expected,
            OnGetAll = () => throw expected,
            OnGet = _ => throw expected,
            OnGetDefault = () => throw expected,
            OnGetSystemStyleId = () => throw expected,
            OnCreate = _ => throw expected,
            OnUpdate = _ => throw expected,
            OnMarkActive = (_, _) => throw expected,
            OnMarkDefault = (_, _) => throw expected
        };
        var logger = new TestLogger();
        var service = new StyleService(repo, logger);

        Exception thrown = methodName switch
        {
            "GetAllActive" => Assert.Throws<InvalidOperationException>(() => service.GetAllActive()),
            "GetAll" => Assert.Throws<InvalidOperationException>(() => service.GetAll()),
            "Get" => Assert.Throws<InvalidOperationException>(() => service.Get(1)),
            "GetDefault" => Assert.Throws<InvalidOperationException>(() => service.GetDefault()),
            "GetSystemStyleId" => Assert.Throws<InvalidOperationException>(() => service.GetSystemStyleId()),
            "Create" => Assert.Throws<InvalidOperationException>(() => service.Create(new StyleModel())),
            "Update" => Assert.Throws<InvalidOperationException>(() => service.Update(new StyleModel())),
            "MarkActive" => Assert.Throws<InvalidOperationException>(() => service.MarkActive(1, true)),
            "MarkDefault" => Assert.Throws<InvalidOperationException>(() => service.MarkDefault(1, true)),
            _ => throw new NotSupportedException(methodName)
        };

        Assert.Same(expected, thrown);
        var entry = Assert.Single(logger.ErrorEntries);
        Assert.Same(expected, entry.Exception);
    }

    private sealed class DummyCustomException(string message) : CustomException(message);
}
