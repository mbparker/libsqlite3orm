using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityDeleterTests
{
    private EntityDeleter _deleter;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteParameterCollection _mockParameters;
    private ISqliteDmlSqlSynthesizer _mockSynthesizer;
    private ISqliteParameterPopulator _mockParameterPopulator;
    private IEntityDetailCache _mockDetailCache;
    private IEntityDetailCacheProvider _mockDetailCacheProvider;
    private ISqliteOrmDatabaseContext _mockContext;
    private Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> _synthesizerFactory;

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private SqliteDbSchema BuildSchema()
    {
        var builder = new SqliteDbSchemaBuilder(new SqliteFieldValueSerialization([], null));
        builder.HasTable<TestEntity>().WithAllMembersAsColumns(x => x.Id).IsAutoIncrement();
        return builder.Build();
    }

    [SetUp]
    public void SetUp()
    {
        _mockConnection = Substitute.For<ISqliteConnection>();
        _mockCommand = Substitute.For<ISqliteCommand>();
        _mockParameters = Substitute.For<ISqliteParameterCollection>();
        _mockSynthesizer = Substitute.For<ISqliteDmlSqlSynthesizer>();
        _mockParameterPopulator = Substitute.For<ISqliteParameterPopulator>();
        _mockDetailCache = Substitute.For<IEntityDetailCache>();
        _mockDetailCacheProvider = Substitute.For<IEntityDetailCacheProvider>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var mockSchema = BuildSchema();
        _mockContext.Schema.Returns(mockSchema);

        _mockConnection.CreateCommand().Returns(_mockCommand);
        _mockCommand.Parameters.Returns(_mockParameters);
        
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();
        
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Delete, Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Delete, mockSchema, null, "DELETE FROM Test WHERE Id = :Id", null);
        _mockSynthesizer.Synthesize<TestEntity>(Arg.Any<SqliteDmlSqlSynthesisArgs>()).Returns(synthesisResult);
        
        _mockDetailCacheProvider.GetCache(default, default).ReturnsForAnyArgs(_mockDetailCache);

        _deleter = new EntityDeleter(
            _synthesizerFactory,
            _mockParameterPopulator,
            _mockDetailCacheProvider,
            _mockContext);
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Delete_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 1;
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(1);

        // Act
        var result = _deleter.Delete(connection, predicate);

        // Assert
        Assert.That(result, Is.EqualTo(1));
        connection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
        _mockParameterPopulator.Received(1).Populate<TestEntity>(Arg.Any<DmlSqlSynthesisResult>(), parameters);
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }

    [Test]
    public void Delete_WithConnectionAndNullPredicate_ThrowsArgumentNullException()
    {
        // Arrange
        var connection = Substitute.For<ISqliteConnection>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _deleter.Delete<TestEntity>(connection, null));
    }

    [Test]
    public void DeleteAll_WithConnection_DoesNotOpenConnection()
    {
        // Arrange
        var connection = Substitute.For<ISqliteConnection>();
        var command = Substitute.For<ISqliteCommand>();
        var parameters = Substitute.For<ISqliteParameterCollection>();
        
        connection.CreateCommand().Returns(command);
        command.Parameters.Returns(parameters);
        command.ExecuteNonQuery(Arg.Any<string>()).Returns(3);

        // Act
        var result = _deleter.DeleteAll<TestEntity>(connection);

        // Assert
        Assert.That(result, Is.EqualTo(3));
        connection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
        command.Received(1).ExecuteNonQuery(Arg.Any<string>());
    }
    
    [Test]
    public void Constructor_InitializesAllDependencies()
    {
        // Act & Assert - Constructor was called in SetUp
        Assert.That(_deleter, Is.Not.Null);
    }
}