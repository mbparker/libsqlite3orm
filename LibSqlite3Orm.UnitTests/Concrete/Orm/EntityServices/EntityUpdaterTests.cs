using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityUpdaterTests
{
    private EntityUpdater _updater;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteParameterCollection _mockParameters;
    private ISqliteDmlSqlSynthesizer _mockSynthesizer;
    private ISqliteParameterPopulator _mockParameterPopulator;
    private ISqliteOrmDatabaseContext _mockContext;
    private Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> _synthesizerFactory;

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        _mockConnection = Substitute.For<ISqliteConnection>();
        _mockCommand = Substitute.For<ISqliteCommand>();
        _mockParameters = Substitute.For<ISqliteParameterCollection>();
        _mockSynthesizer = Substitute.For<ISqliteDmlSqlSynthesizer>();
        _mockParameterPopulator = Substitute.For<ISqliteParameterPopulator>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var mockSchema = Substitute.For<SqliteDbSchema>();
        _mockContext.Schema.Returns(mockSchema);

        _mockConnection.CreateCommand().Returns(_mockCommand);
        _mockCommand.Parameters.Returns(_mockParameters);
        
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();
        
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Update, Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Update, mockSchema, null, "UPDATE Test SET Name = :Name WHERE Id = :Id", null);
        _mockSynthesizer.Synthesize<TestEntity>(Arg.Any<SqliteDmlSqlSynthesisArgs>()).Returns(synthesisResult);

        _updater = new EntityUpdater(
            _synthesizerFactory,
            _mockParameterPopulator,
            _mockContext);
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Update_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _updater.Update(connection, entity);

        // Assert
        Assert.That(result, Is.True);
        connection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
        _mockParameterPopulator.Received(1).Populate(Arg.Any<DmlSqlSynthesisResult>(), parameters, entity);
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Update_WithConnectionAndSynthesisResult_UsesProvidedResult()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Update, _mockContext.Schema, null, "UPDATE Test SET Name = :Name WHERE Id = :Id", null);
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery("UPDATE Test SET Name = :Name WHERE Id = :Id").Returns(1);

        // Act
        var result = _updater.Update(connection, synthesisResult, entity);

        // Assert
        Assert.That(result, Is.True);
        _mockParameterPopulator.Received(1).Populate(synthesisResult, parameters, entity);
        command.Received(1).ExecuteNonQuery("UPDATE Test SET Name = :Name WHERE Id = :Id");
    }

    [Test]
    public void Update_WithConnectionAndSynthesisResult_WhenNoRecordAffected_ReturnsFalse()
    {
        // Arrange
        var entity = new TestEntity { Id = 999, Name = "NonExistent" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Update, _mockContext.Schema, null, "UPDATE Test SET Name = :Name WHERE Id = :Id", null);
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(0);

        // Act
        var result = _updater.Update(connection, synthesisResult, entity);

        // Assert
        Assert.That(result, Is.False);
    }
}