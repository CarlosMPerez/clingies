using System.Data;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Clingies.Infrastructure.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Clingies.Tests.Integration
{
    [Collection("ClingiesRepositoriesTests")]
    public class StyleRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly StyleRepository _repository;

        private const string COLOR_BLACK = "#000000";
        private const string COLOR_WHITE = "#FFFFFF";
        private const string COLOR_YELLOW = "#FFFF00";
        private const string COLOR_BLUE = "#0018F9";
        private const string COLOR_RED = "#D30000";

        public StyleRepositoryTests()
        {
            // Use in-memory SQLite, keep connection open for test duration
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
                _connection.Open();
            }

            // Create schema 
            var schema = File.ReadAllText("schema_styles.sql");
            _connection.Execute(schema);

            // Setup ConnectionFactory that returns this in-memory connection
            _connectionFactory = new InMemoryConnectionFactory(_connection);
            _repository = new StyleRepository(_connectionFactory, new TestLogger());
        }

        [Fact]
        public void Can_Create_And_Read_Style()
        {
            var def = CreateDefaultTestStyle();
            _repository.Create(def);
            var sut = _repository.Get("default");

            Assert.NotNull(sut);
            Assert.Equal("Arial", sut.BodyFont);
            Assert.Equal(COLOR_YELLOW, sut.BodyColor);
            Assert.Equal("none;", sut.BodyFontDecorations);
            Assert.Equal(10, sut.TitleFontSize);
        }

        [Fact]
        public void Can_Get_All()
        {
            var def = CreateDefaultTestStyle();
            _repository.Create(def);
            var light = CreateLightTestStyle();
            _repository.Create(light);
            var dark = CreateDarkTestStyle();
            _repository.Create(dark);

            var sut = _repository.GetAll();

            Assert.NotNull(sut);
            Assert.Equal(3, sut.Count);

            var item = sut.Single(x => x.Id == "dark");
            Assert.NotNull(item);
            Assert.Equal("Papyrus", item.BodyFont);
            Assert.Equal(COLOR_BLACK, item.BodyColor);
            Assert.Equal("underscore;", item.BodyFontDecorations);
            Assert.Equal(14, item.TitleFontSize);
        }

        [Fact]
        public void Can_Get_Style_By_Id()
        {
            var light = CreateLightTestStyle();
            _repository.Create(light);

            var sut = _repository.Get("light");

            Assert.NotNull(sut);
            Assert.Equal("Hack", sut.BodyFont);
            Assert.Equal(COLOR_RED, sut.BodyColor);
            Assert.Equal("none;", sut.BodyFontDecorations);
            Assert.Equal(12, sut.TitleFontSize);
        }

        [Fact]
        public void Can_Update_Style()
        {
            _repository.Create(CreateLightTestStyle());

            var upd = new Style
            {
                Id = "light",
                BodyColor = COLOR_YELLOW,
                TitleColor = COLOR_BLACK,
                BodyFont = "sans-serif",
                BodyFontColor = COLOR_BLUE,
                BodyFontSize = 22,
                BodyFontDecorations = "bold;",
                TitleFont = "roboto",
                TitleFontSize = 15,
                TitleFontColor = COLOR_BLUE,
                TitleFontDecorations = "italics;"
            };

            Console.WriteLine("Updating with TitleFontColor: " + upd.TitleFontColor);

            _repository.Update(upd);

            var sut = _repository.Get("light");

            Assert.NotNull(sut);
            Assert.Equal("light", sut.Id);
            Assert.Equal(COLOR_YELLOW, sut.BodyColor);
            Assert.Equal(COLOR_BLACK, sut.TitleColor);
            Assert.Equal("sans-serif", sut.BodyFont);
            Assert.Equal(COLOR_BLUE, sut.BodyFontColor);
            Assert.Equal(22, sut.BodyFontSize);
            Assert.Equal("bold;", sut.BodyFontDecorations);
            Assert.Equal("roboto", sut.TitleFont);
            Assert.Equal(15, sut.TitleFontSize);
            Assert.Equal(COLOR_BLUE, sut.TitleFontColor);
            Assert.Equal("italics;", sut.TitleFontDecorations);
        }

        [Fact]
        public void Can_Delete_Style()
        {
            var def = CreateDefaultTestStyle();
            _repository.Delete("default");

            var sut = _repository.Get("default");

            Assert.Null(sut);
        }


        public void Dispose()
        {
            _connection.Execute("DELETE FROM styles;");
            _connection.Execute("VACUUM;");
        }

        // A simple test logger that does nothing (or you can log to test output)
        private class TestLogger : IClingiesLogger
        {
            public void Debug(string message, params object[] args) { }
            public void Info(string message, params object[] args) { }
            public void Error(Exception ex, string message, params object[] args) { }
            public void Warning(string message, params object[] args) { }
        }

        // A test connection factory that always returns the same open connection
        private class InMemoryConnectionFactory : IConnectionFactory
        {
            private readonly SqliteConnection _connection;

            public InMemoryConnectionFactory(SqliteConnection connection)
            {
                _connection = connection;
            }

            public IDbConnection GetConnection()
            {
                return _connection;
            }
        }

        private Style CreateDefaultTestStyle()
        {
            return new Style
            {
                Id = "default",
                BodyColor = COLOR_YELLOW,
                TitleColor = COLOR_BLACK,
                BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK,
                BodyFontSize = 10,
                BodyFontDecorations = "none;",
                TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK,
                TitleFontSize = 10,
                TitleFontDecorations = "bold;"
            };
        }

        private Style CreateLightTestStyle()
        {
            return new Style
            {
                Id = "light",
                BodyColor = COLOR_RED,
                TitleColor = COLOR_BLUE,
                BodyFont = "Hack",
                BodyFontColor = COLOR_BLACK,
                BodyFontSize = 10,
                BodyFontDecorations = "none;",
                TitleFont = "Times News Roman",
                TitleFontColor = COLOR_BLACK,
                TitleFontSize = 12,
                TitleFontDecorations = "bold;"
            };
        }

        private Style CreateDarkTestStyle()
        {
            return new Style
            {
                Id = "dark",
                BodyColor = COLOR_BLACK,
                TitleColor = COLOR_BLACK,
                BodyFont = "Papyrus",
                BodyFontColor = COLOR_WHITE,
                BodyFontSize = 10,
                BodyFontDecorations = "underscore;",
                TitleFont = "Windigs",
                TitleFontColor = COLOR_WHITE,
                TitleFontSize = 14,
                TitleFontDecorations = "bold;"
            };
        }
    }
}
