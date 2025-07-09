using System.Data;
using Clingies.Infrastructure.Data; // assuming ClingyRepository + IConnectionFactory here
using Dapper;
using Microsoft.Data.Sqlite;
using Clingies.Common;
using Clingies.Domain.Models;
using Clingies.Domain.Factories;

namespace Clingies.Tests.Integration
{
    [Collection("RepositoryTests")]
    public class RepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ClingyRepository _repository;

        public RepositoryTests()
        {
            // Use in-memory SQLite, keep connection open for test duration
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
                _connection.Open();
            }

            // Create schema 
            var schema = File.ReadAllText("schema.sql");
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
        public void Can_Update_Clingy()
        {
            var clingy = CreateTestClingy();
            var id = _repository.Create(clingy);

            clingy.UpdateTitle("UPDATED TITLE");
            clingy.UpdateContent("This is an updated content");
            clingy.SetPinState(true);
            clingy.SetRolledState(true);
            _repository.Update(clingy);

            var sut = _repository.Get(id);

            Assert.NotNull(sut);
            Assert.Equal("UPDATED TITLE", sut.Title);
            Assert.Equal("This is an updated content", sut.Content);
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
            _connection.Execute("DELETE FROM Clingies;");
            _connection.Execute("VACUUM;");
        }

        // A simple test logger that does nothing (or you can log to test output)
        private class TestLogger : IClingiesLogger
        {
            public void Debug(string message, params object[] args) { }
            public void Info(string message, params object[] args) { }
            public void Error(Exception ex, string message, params object[] args) { }
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
            return ClingyEntityFactory.CreateNew("TEST CLINGY",
            "This is a clingy text content for tests", 100, 200);
        }

        private Clingy CreateSoftDeletedTestClingy()
        {
            var clg = ClingyEntityFactory.CreateNew("TEST CLINGY",
                    "This is a clingy text content for tests", 100, 200);
            clg.MarkDeleted();
            return clg;
        }
    }
}