using System.Data;
using Clingies.Domain.Interfaces;
using Clingies.Domain.Models;
using Clingies.Infrastructure.CustomExceptions;
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
            var def = ReturnActiveDefaultTestStyle();
            _repository.Create(def);
            var sut = _repository.Get("default_test");

            Assert.NotNull(sut);
            Assert.Equal("Arial", sut.BodyFont);
            Assert.Equal(COLOR_YELLOW, sut.BodyColor);
            Assert.Equal("none;", sut.BodyFontDecorations);
            Assert.Equal(10, sut.TitleFontSize);
        }

        [Fact]
        public void Cannot_Create_Styles_With_Repeated_Names()
        {
            var def = ReturnActiveDefaultTestStyle();
            _repository.Create(def);
            var def2 = ReturnActiveDefaultTestStyle();

            var ex = Assert.Throws<SqliteException>(() =>
            {
                _repository.Create(def2);
            });

            Assert.Equal(19, ex.SqliteErrorCode);
            Assert.Contains("UNIQUE", ex.Message.ToUpperInvariant());
            Assert.Contains("id", ex.Message);
        }

        [Fact]
        public void Cannot_Have_More_Than_10_Active_Styles()
        {
            Create10ActiveStyles();
            var sut = ReturnActiveDefaultTestStyle();

            Assert.Throws<TooManyActiveStylesException>(() =>
            {
                _repository.Create(sut);
            });
        }

        [Fact]
        public void Cannot_Delete_Active_Style()
        {
            _repository.Create(ReturnActiveDefaultTestStyle());
            Assert.Throws<CannotDeleteActiveStyle>(() =>
            {
                _repository.Delete("default_test");
            });
        }

        [Fact]
        public void Cannot_Delete_System_Style()
        {
            _repository.Create(ReturnSystemTestStyle());
            Assert.Throws<CannotDeleteSystemStyleException>(() =>
            {
                _repository.Delete("system");
            });
        }

        [Fact]
        public void Cannot_Update_System_Style()
        {
            var sut = ReturnSystemTestStyle();
            _repository.Create(sut);

            sut.IsActive = false;
            sut.BodyColor = COLOR_RED;
            sut.TitleColor = COLOR_YELLOW;

            Assert.Throws<CannotUpdateSystemStyleException>(() =>
            {
                _repository.Update(sut);
            });
        }

        [Fact]
        public void Can_Get_All()
        {
            var def = ReturnActiveDefaultTestStyle();
            _repository.Create(def);
            var light = ReturnLightTestStyle();
            _repository.Create(light);
            var dark = ReturnDarkTestStyle();
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
            var light = ReturnLightTestStyle();
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
            _repository.Create(ReturnLightTestStyle());

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
                TitleFontDecorations = "italics;",
                IsActive = true,
                IsDefault = false
            };

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
            var def = ReturnDarkTestStyle();
            _repository.Create(def);
            _repository.Delete("dark");

            var sut = _repository.Get("dark");

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

        private Style ReturnSystemTestStyle()
        {
            return new Style
            {
                Id = "system",
                BodyColor = COLOR_YELLOW,
                TitleColor = COLOR_BLACK,
                BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK,
                BodyFontSize = 10,
                BodyFontDecorations = "none;",
                TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK,
                TitleFontSize = 10,
                TitleFontDecorations = "bold;",
                IsActive = true,
                IsDefault = true
            };
        }
        private Style ReturnActiveDefaultTestStyle()
        {
            return new Style
            {
                Id = "default_test",
                BodyColor = COLOR_YELLOW,
                TitleColor = COLOR_BLACK,
                BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK,
                BodyFontSize = 10,
                BodyFontDecorations = "none;",
                TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK,
                TitleFontSize = 10,
                TitleFontDecorations = "bold;",
                IsActive = true,
                IsDefault = true
            };
        }

        private void Create10ActiveStyles()
        {
            _repository.Create(new Style { Id = "s1", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s2", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s3", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s4", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s5", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s6", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s7", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s8", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s9", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
            _repository.Create(new Style { Id = "s10", BodyColor = COLOR_YELLOW, TitleColor = COLOR_BLACK, BodyFont = "Arial",
                BodyFontColor = COLOR_BLACK, BodyFontSize = 10, BodyFontDecorations = "none;", TitleFont = "Verdana",
                TitleFontColor = COLOR_BLACK, TitleFontSize = 10, TitleFontDecorations = "bold;", IsActive = true, IsDefault = false });
        }

        private Style ReturnLightTestStyle()
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
                TitleFontDecorations = "bold;",
                IsActive = true,
                IsDefault = false
            };
        }

        private Style ReturnDarkTestStyle()
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
                TitleFontDecorations = "bold;",
                IsActive = false,
                IsDefault = false
            };
        }
    }
}
