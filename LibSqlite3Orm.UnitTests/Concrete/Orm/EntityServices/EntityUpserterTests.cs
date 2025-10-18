using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityUpserterTests
{
    private EntityUpserter _upserter;
    private ISqliteConnection _mockConnection;
    private IEntityCreator _mockCreator;
    private IEntityUpdater _mockUpdater;
    private ISqliteDmlSqlSynthesizer _mockInsertSynthesizer;
    private ISqliteDmlSqlSynthesizer _mockUpdateSynthesizer;
    private DmlSqlSynthesisResult _updateSynthesisResult;
    private DmlSqlSynthesisResult _insertSynthesisResult;
    private ISqliteOrmDatabaseContext _mockContext;
    private Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> _synthesizerFactory;

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        _mockConnection = Substitute.For<ISqliteConnection>();
        _mockCreator = Substitute.For<IEntityCreator>();
        _mockUpdater = Substitute.For<IEntityUpdater>();
        _mockInsertSynthesizer = Substitute.For<ISqliteDmlSqlSynthesizer>();
        _mockUpdateSynthesizer = Substitute.For<ISqliteDmlSqlSynthesizer>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var mockSchema = Substitute.For<SqliteDbSchema>();
        _mockContext.Schema.Returns(mockSchema);
        
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();

        var creatorFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityCreator>>();
        var updaterFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityUpdater>>();
        
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Insert, Arg.Any<SqliteDbSchema>()).Returns(_mockInsertSynthesizer);
        _synthesizerFactory.Invoke(SqliteDmlSqlSynthesisKind.Update, Arg.Any<SqliteDbSchema>()).Returns(_mockUpdateSynthesizer);
        creatorFactory.Invoke(_mockContext).Returns(_mockCreator);
        updaterFactory.Invoke(_mockContext).Returns(_mockUpdater);

        _updateSynthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Update, new SqliteDbSchema(),
            new SqliteDbSchemaTable(), string.Empty, null);
        _mockUpdateSynthesizer.Synthesize<TestEntity>(default).ReturnsForAnyArgs(_updateSynthesisResult);
        _insertSynthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, new SqliteDbSchema(),
            new SqliteDbSchemaTable(), string.Empty, null);
        _mockInsertSynthesizer.Synthesize<TestEntity>(default).ReturnsForAnyArgs(_insertSynthesisResult);

        _upserter = new EntityUpserter(
            _synthesizerFactory,
            creatorFactory,
            updaterFactory,
            _mockContext);
    }

    [Test]
    public void UpsertWithConnection_WhenUpdateSucceeds_ReturnsUpdated()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        _mockUpdater.Update(connection, entity).Returns(true);

        // Act
        var result = _upserter.Upsert(connection, entity);

        // Assert
        Assert.That(result, Is.EqualTo(UpsertResult.Updated));
        _mockUpdater.Received(1).Update(connection, entity);
        _mockCreator.DidNotReceive().Insert(Arg.Any<ISqliteConnection>(), Arg.Any<TestEntity>());
    }

    [Test]
    public void UpsertWithConnection_WhenUpdateFailsAndInsertSucceeds_ReturnsInserted()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        _mockUpdater.Update(connection, entity).Returns(false);
        _mockCreator.Insert(connection, entity).Returns(true);

        // Act
        var result = _upserter.Upsert(connection, entity);

        // Assert
        Assert.That(result, Is.EqualTo(UpsertResult.Inserted));
        _mockUpdater.Received(1).Update(connection, entity);
        _mockCreator.Received(1).Insert(connection, entity);
    }

    [Test]
    public void UpsertWithConnection_WhenBothUpdateAndInsertFail_ReturnsFailed()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        _mockUpdater.Update(connection, entity).Returns(false);
        _mockCreator.Insert(connection, entity).Returns(false);

        // Act
        var result = _upserter.Upsert(connection, entity);

        // Assert
        Assert.That(result, Is.EqualTo(UpsertResult.Failed));
        _mockUpdater.Received(1).Update(connection, entity);
        _mockCreator.Received(1).Insert(connection, entity);
    }

    [Test]
    public void UpsertManyWithConnection_WithEntities_UsesProvidedConnection()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };
        var connection = Substitute.For<ISqliteConnection>();

        _mockUpdater.Update(connection, _updateSynthesisResult, entities[0]).Returns(true);
        _mockUpdater.Update(connection, _updateSynthesisResult, entities[1]).Returns(false);
        _mockCreator.Insert(connection, _insertSynthesisResult, entities[1]).Returns(true);

        // Act
        var result = _upserter.UpsertMany(connection, entities);

        // Assert
        Assert.That(result.UpdateCount, Is.EqualTo(1));
        Assert.That(result.InsertCount, Is.EqualTo(1));
        Assert.That(result.FailedCount, Is.EqualTo(0));

        _mockUpdater.Received(2).Update(connection, _updateSynthesisResult, Arg.Any<TestEntity>());
        _mockCreator.Received(1).Insert(connection, _insertSynthesisResult, Arg.Any<TestEntity>());
    }

    [Test]
    public void Constructor_InitializesSynthesizers()
    {
        // Assert
        _synthesizerFactory.Received(1).Invoke(SqliteDmlSqlSynthesisKind.Insert, Arg.Any<SqliteDbSchema>());
        _synthesizerFactory.Received(1).Invoke(SqliteDmlSqlSynthesisKind.Update, Arg.Any<SqliteDbSchema>());
    }
}