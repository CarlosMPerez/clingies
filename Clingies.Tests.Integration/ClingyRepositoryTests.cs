using System.Data;
using Clingies.Infrastructure.Data; // assuming ClingyRepository + IConnectionFactory here
using Dapper;
using Microsoft.Data.Sqlite;
using Clingies.Domain.Models;
using Clingies.Domain.Interfaces;

namespace Clingies.Tests.Integration
{
    [Collection("ClingiesRepositoriesTests")]
    public class ClingyRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ClingyRepository _repository;

        public ClingyRepositoryTests()
        {
            // Use in-memory SQLite, keep connection open for test duration
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
                _connection.Open();
            }

            // Create schema 
            var schema = File.ReadAllText("schema_clingies.sql");
            _connection.Execute(schema);

            // Setup ConnectionFactory that returns this in-memory connection
            _connectionFactory = new InMemoryConnectionFactory(_connection);
            _repository = new ClingyRepository(_connectionFactory, new TestLogger());
        }

        [Fact]
        public void Can_Create_And_Read_Clingy()
        {
            var clingy = CreateTestClingy();
            var id = _repository.Create(clingy);
            var sut = _repository.Get(id);

            Assert.NotNull(sut);
            Assert.Equal("TEST CLINGY", sut!.Title);
            Assert.Equal("This is a clingy text content for tests", sut.Content);
            Assert.Equal("default", sut.Style);
            Assert.Equal(100, sut.PositionX);
            Assert.Equal(200, sut.PositionY);
        }

        [Fact]
        public void Can_Get_All_Active()
        {
            var clingy1 = CreateTestClingy();
            _repository.Create(clingy1);
            var clingy2 = CreateTestClingy();
            _repository.Create(clingy2);
            var clingy3 = CreateSoftDeletedTestClingy();
            _repository.Create(clingy3);

            var sut = _repository.GetAllActive();

            Assert.NotNull(sut);
            Assert.Equal(2, sut.Count);
        }

        [Fact]
        public void Can_Get_Clingy_By_Id()
        {
            var clingy = CreatedDetailedTestClingy();
            int id = _repository.Create(clingy);

            var sut = _repository.Get(id);

            Assert.NotNull(sut);
            //"", ""
            Assert.Equal("DETAILED CLINGY", sut.Title);
            Assert.Equal("This is a detailed clingy for tests", sut.Content);
            Assert.Equal(250, sut.Width);
            Assert.Equal(500, sut.Height);
            Assert.Equal(500, sut.PositionX);
            Assert.Equal(750, sut.PositionY);
            Assert.True(sut.IsLocked);
            Assert.True(sut.IsPinned);
            Assert.False(sut.IsDeleted);
            Assert.Equal("dark", sut.Style);
        }

        [Fact]
        public void Can_Update_Clingy()
        {
            var clingy = CreateTestClingy();
            var id = _repository.Create(clingy);

            clingy.Id = id;
            clingy.Title = "UPDATED TITLE";
            clingy.Content = "This is an updated content";
            clingy.IsPinned = true;
            clingy.IsRolled = true;
            clingy.Style = "lemon";
            _repository.Update(clingy);

            var sut = _repository.Get(id);
            Assert.NotNull(sut);
            Assert.Equal("UPDATED TITLE", sut.Title);
            Assert.Equal("This is an updated content", sut.Content);
            Assert.Equal("lemon", sut.Style);
            Assert.True(sut.IsRolled);
            Assert.True(sut.IsPinned);
        }

        [Fact]
        public void Can_Soft_Delete_Clingy()
        {
            var clingy = CreateTestClingy();
            var id = _repository.Create(clingy);

            _repository.SoftDelete(id);

            var sut = _repository.Get(id);
            Assert.NotNull(sut);
            Assert.True(sut.IsDeleted);
        }

        [Fact]
        public void Can_Hard_Delete_Clingy()
        {
            var clingy = CreateTestClingy();
            var id = _repository.Create(clingy);

            _repository.HardDelete(id);

            var sut = _repository.Get(id);

            Assert.Null(sut);
        }

        public void Dispose()
        {
            _connection.Execute("DELETE FROM clingies;");
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

        private Clingy CreateTestClingy()
        {
            return new Clingy
            {
                Title = "TEST CLINGY",
                Content = "This is a clingy text content for tests",
                PositionX = 100,
                PositionY = 200
            };
        }

        private Clingy CreatedDetailedTestClingy()
        {
            return new Clingy
            {
                Title = "DETAILED CLINGY",
                Content = "This is a detailed clingy for tests",
                PositionX = 500,
                PositionY = 750,
                Width = 250,
                Height = 500,
                Style = "dark",
                IsLocked = true,
                IsPinned = true,
            };
        }

        private Clingy CreateSoftDeletedTestClingy()
        {
            return new Clingy
            {
                Title = "TEST CLINGY",
                Content = "This is a clingy text content for tests",
                PositionX = 100,
                PositionY = 200,
                IsDeleted = true
            };
        }
    }
}