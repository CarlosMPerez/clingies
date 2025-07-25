using System.Data;
using Clingies.Infrastructure.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Clingies.Domain.Interfaces;

namespace Clingies.Tests.Integration
{
    [Collection("ClingiesRepositoriesTests")]
    public class MenuRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly IConnectionFactory _connectionFactory;
        private readonly MenuRepository _repository;

        public MenuRepositoryTests()
        {
            // Use in-memory SQLite, keep connection open for test duration
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
                _connection.Open();
            }

            // Create schema 
            var schema = File.ReadAllText("schema_menu.sql");
            _connection.Execute(schema);

            // Setup ConnectionFactory that returns this in-memory connection
            _connectionFactory = new InMemoryConnectionFactory(_connection);
            _repository = new MenuRepository(_connectionFactory, new TestLogger());
        }

        [Fact]
        public void Can_GetAllParents_Tray()
        {
            CreateTestMenuSystem();
            var sut = _repository.GetAllParents("tray");
            var item = sut.Where(x => x.Id == "tray9").Single();

            Assert.NotNull(sut);
            Assert.Equal(10, sut.Count);
            Assert.Equal("label9", item.Label);
            Assert.Equal("tooltip9", item.Tooltip);
        }

        [Fact]
        public void Can_GetAllParents_Clingy()
        {
            CreateTestMenuSystem();
            var sut = _repository.GetAllParents("clingy");
            var item = sut.Where(x => x.Id == "clingy2").Single();

            Assert.NotNull(sut);
            Assert.Equal(5, sut.Count);
            Assert.Equal("label2", item.Label);
            Assert.Equal("tooltip2", item.Tooltip);
        }

        [Fact]
        public void Can_GetChildrenByParentId_Tray()
        {
            CreateTestMenuSystem();
            var sut = _repository.GetChildrenByParentId("tray5");
            var item = sut.Where(x => x.Id == "traychild4").Single();

            Assert.NotEmpty(sut);
            Assert.Equal(5, sut.Count);
            Assert.Equal("label5_4", item.Label);
            Assert.Equal("tooltip5_4", item.Tooltip);
        }

        [Fact]
        public void Can_GetChildrenByParentId_Clingy()
        {
            CreateTestMenuSystem();
            var sut = _repository.GetChildrenByParentId("clingy3");
            var item = sut.Where(x => x.Id == "clingychild2").Single();

            Assert.NotEmpty(sut);
            Assert.Equal(3, sut.Count);
            Assert.Equal("label3_2", item.Label);
            Assert.Equal("tooltip3_2", item.Tooltip);
        }

        public void Dispose()
        {
            _connection.Execute("DELETE FROM system_menu;");
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

        private void CreateTestMenuSystem()
        {
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray1', 'tray', null, 'label1', 'tooltip1', true, false, 10);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray2', 'tray', null, 'label2', 'tooltip2', true, false, 20);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray3', 'tray', null, 'label3', 'tooltip3', true, false, 30);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray4', 'tray', null, 'label4', 'tooltip4', true, false, 40);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray5', 'tray', null, 'label5', 'tooltip5', true, false, 50);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray6', 'tray', null, 'label6', 'tooltip6', true, false, 60);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray7', 'tray', null, 'label7', 'tooltip7', true, false, 70);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray8', 'tray', null, 'label8', 'tooltip8', true, false, 80);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray9', 'tray', null, 'label9', 'tooltip9', true, false, 90);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('tray10', 'tray', null, 'label10', 'tooltip10', true, false, 100);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('traychild1', 'tray', 'tray5', 'label5_1', 'tooltip5_1', true, false, 10);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('traychild2', 'tray', 'tray5', 'label5_2', 'tooltip5_2', true, false, 20);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('traychild3', 'tray', 'tray5', 'label5_3', 'tooltip5_3', true, false, 30);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('traychild4', 'tray', 'tray5', 'label5_4', 'tooltip5_4', true, false, 40);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('traychild5', 'tray', 'tray5', 'label5_5', 'tooltip5_5', true, false, 50);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingy1', 'clingy', null, 'label1', 'tooltip1', true, false, 10);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingy2', 'clingy', null, 'label2', 'tooltip2', true, false, 20);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingy3', 'clingy', null, 'label3', 'tooltip3', true, false, 30);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingy4', 'clingy', null, 'label4', 'tooltip4', true, false, 40);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingy5', 'clingy', null, 'label5', 'tooltip5', true, false, 50);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingychild1', 'clingy', 'clingy3', 'label3_1', 'tooltip3_1', true, false, 10);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingychild2', 'clingy', 'clingy3', 'label3_2', 'tooltip3_2', true, false, 20);");
            _connection.Execute("INSERT INTO system_menu (id, menu_type, parent_id, label, tooltip, enabled, separator, sort_order) VALUES ('clingychild3', 'clingy', 'clingy3', 'label3_3', 'tooltip3_3', true, false, 30);");
        }
    }
}