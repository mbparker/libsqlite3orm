using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityCreatorTests
{
    private EntityCreator _creator;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteParameterCollection _mockParameters;
    private ISqliteDmlSqlSynthesizer _mockSynthesizer;
    private ISqliteParameterPopulator _mockParameterPopulator;
    private ISqliteEntityPostInsertPrimaryKeySetter _mockPrimaryKeySetter;
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
        _mockPrimaryKeySetter = Substitute.For<ISqliteEntityPostInsertPrimaryKeySetter>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var mockSchema = Substitute.For<SqliteDbSchema>();
        _mockContext.Schema.Returns(mockSchema);

        _mockConnection.CreateCommand().Returns(_mockCommand);
        _mockCommand.Parameters.Returns(_mockParameters);
        
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();
        
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Insert, Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, mockSchema, null, "INSERT INTO Test VALUES (1)", null);
        _mockSynthesizer.Synthesize<TestEntity>(Arg.Any<SqliteDmlSqlSynthesisArgs>()).Returns(synthesisResult);

        _creator = new EntityCreator(
            _synthesizerFactory,
            _mockParameterPopulator,
            _mockPrimaryKeySetter,
            _mockContext);
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Insert_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _creator.Insert(connection, entity);

        // Assert
        Assert.That(result, Is.True);
        connection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
        _mockParameterPopulator.Received(1).Populate(Arg.Any<DmlSqlSynthesisResult>(), parameters, entity);
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Insert_WithConnectionAndSynthesisResult_UsesProvidedResult()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, _mockContext.Schema, null, "INSERT INTO Test VALUES (:Name)", null);
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery("INSERT INTO Test VALUES (:Name)").Returns(1);

        // Act
        var result = _creator.Insert(connection, synthesisResult, entity);

        // Assert
        Assert.That(result, Is.True);
        _mockParameterPopulator.Received(1).Populate(synthesisResult, parameters, entity);
        command.Received(1).ExecuteNonQuery("INSERT INTO Test VALUES (:Name)");
        _mockPrimaryKeySetter.Received(1).SetAutoIncrementedPrimaryKeyOnEntityIfNeeded(_mockContext.Schema, connection, entity);
    }

    [Test]
    public void InsertMany_WithConnection_UsesProvidedConnection()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _creator.InsertMany(connection, entities);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        connection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
        command.Received(2).ExecuteNonQuery(Arg.Any<string>());
    }
}