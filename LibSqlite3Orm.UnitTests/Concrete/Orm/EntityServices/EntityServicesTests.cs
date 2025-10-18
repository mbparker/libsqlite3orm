using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.EntityServices;

[TestFixture]
public class LibSqliteEntityServicesTests
{
    private LibSqlite3Orm.Concrete.Orm.EntityServices.EntityServices _entityServices;
    private IEntityCreator _mockCreator;
    private IEntityGetter _mockGetter;
    private IEntityUpdater _mockUpdater;
    private IEntityDeleter _mockDeleter;
    private IEntityUpserter _mockUpserter;
    private ISqliteOrmDatabaseContext _mockContext;

    [SetUp]
    public void SetUp()
    {
        _mockCreator = Substitute.For<IEntityCreator>();
        _mockGetter = Substitute.For<IEntityGetter>();
        _mockUpdater = Substitute.For<IEntityUpdater>();
        _mockDeleter = Substitute.For<IEntityDeleter>();
        _mockUpserter = Substitute.For<IEntityUpserter>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();

        var creatorFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityCreator>>();
        var getterFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityGetter>>();
        var updaterFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityUpdater>>();
        var deleterFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityDeleter>>();
        var upserterFactory = Substitute.For<Func<ISqliteOrmDatabaseContext, IEntityUpserter>>();

        creatorFactory.Invoke(_mockContext).Returns(_mockCreator);
        getterFactory.Invoke(_mockContext).Returns(_mockGetter);
        updaterFactory.Invoke(_mockContext).Returns(_mockUpdater);
        deleterFactory.Invoke(_mockContext).Returns(_mockDeleter);
        upserterFactory.Invoke(_mockContext).Returns(_mockUpserter);

        _entityServices = new LibSqlite3Orm.Concrete.Orm.EntityServices.EntityServices(
            creatorFactory,
            updaterFactory,
            upserterFactory,
            getterFactory,
            deleterFactory,
            _mockContext);
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    
    [Test]
    public void Insert_WithConnectionAndEntity_CallsCreatorInsert()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        _mockCreator.Insert(connection, entity).Returns(true);

        // Act
        var result = _entityServices.Insert(connection, entity);

        // Assert
        Assert.That(result, Is.True);
        _mockCreator.Received(1).Insert(connection, entity);
    }

    [Test]
    public void Insert_WithConnectionSynthesisResultAndEntity_CallsCreatorInsert()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var connection = Substitute.For<ISqliteConnection>();
        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, new SqliteDbSchema(),
            new SqliteDbSchemaTable(), string.Empty, null);
        _mockCreator.Insert(connection, synthesisResult, entity).Returns(true);

        // Act
        var result = _entityServices.Insert(connection, synthesisResult, entity);

        // Assert
        Assert.That(result, Is.True);
        _mockCreator.Received(1).Insert(connection, synthesisResult, entity);
    }

    [Test]
    public void InsertMany_WithConnectionAndEntities_CallsCreatorInsertMany()
    {
        // Arrange
        var entities = new[] { new TestEntity { Id = 1, Name = "Test1" }, new TestEntity { Id = 2, Name = "Test2" } };
        var connection = Substitute.For<ISqliteConnection>();
        _mockCreator.InsertMany(connection, entities).Returns(2);

        // Act
        var result = _entityServices.InsertMany(connection, entities);

        // Assert
        Assert.That(result, Is.EqualTo(2));
        _mockCreator.Received(1).InsertMany(connection, entities);
    }

    [Test]
    public void Get_WithDefaultParams_CallsGetterGet()
    {
        // Arrange
        var mockQueryable = Substitute.For<ISqliteQueryable<TestEntity>>();
        var connection = Substitute.For<ISqliteConnection>();
        _mockGetter.Get<TestEntity>(connection, false).Returns(mockQueryable);

        // Act
        var result = _entityServices.Get<TestEntity>(connection);

        // Assert
        Assert.That(result, Is.EqualTo(mockQueryable));
        _mockGetter.Received(1).Get<TestEntity>(connection, false);
    }

    [Test]
    public void Get_WithIncludeDetails_CallsGetterGet()
    {
        // Arrange
        var mockQueryable = Substitute.For<ISqliteQueryable<TestEntity>>();
        var connection = Substitute.For<ISqliteConnection>();
        _mockGetter.Get<TestEntity>(connection, true).Returns(mockQueryable);

        // Act
        var result = _entityServices.Get<TestEntity>(connection, true);

        // Assert
        Assert.That(result, Is.EqualTo(mockQueryable));
        _mockGetter.Received(1).Get<TestEntity>(connection, true);
    }

    [Test]
    public void Get_WithConnection_CallsGetterGet()
    {
        // Arrange
        var connection = Substitute.For<ISqliteConnection>();
        var mockQueryable = Substitute.For<ISqliteQueryable<TestEntity>>();
        _mockGetter.Get<TestEntity>(connection, false).Returns(mockQueryable);

        // Act
        var result = _entityServices.Get<TestEntity>(connection);

        // Assert
        Assert.That(result, Is.EqualTo(mockQueryable));
        _mockGetter.Received(1).Get<TestEntity>(connection, false);
    }

    [Test]
    public void Update_WithConnectionAndEntity_CallsUpdaterUpdate()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated" };
        var connection = Substitute.For<ISqliteConnection>();
        _mockUpdater.Update(connection, entity).Returns(true);

        // Act
        var result = _entityServices.Update(connection, entity);

        // Assert
        Assert.That(result, Is.True);
        _mockUpdater.Received(1).Update(connection, entity);
    }

    [Test]
    public void Delete_WithConnectionAndPredicate_CallsDeleterDelete()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> predicate = e => e.Id == 1;
        var connection = Substitute.For<ISqliteConnection>();
        _mockDeleter.Delete(connection, predicate).Returns(1);

        // Act
        var result = _entityServices.Delete(connection, predicate);

        // Assert
        Assert.That(result, Is.EqualTo(1));
        _mockDeleter.Received(1).Delete(connection, predicate);
    }

    [Test]
    public void Upsert_WithConnectionAndEntity_CallsUpserterUpsert()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Upsert" };
        var connection = Substitute.For<ISqliteConnection>();
        _mockUpserter.Upsert(connection, entity).Returns(UpsertResult.Updated);

        // Act
        var result = _entityServices.Upsert(connection, entity);

        // Assert
        Assert.That(result, Is.EqualTo(UpsertResult.Updated));
        _mockUpserter.Received(1).Upsert(connection, entity);
    }

    [Test]
    public void UpsertMany_WithConnectionAndEntities_CallsUpserterUpsertMany()
    {
        // Arrange
        var entities = new[] { new TestEntity { Id = 1, Name = "Test1" }, new TestEntity { Id = 2, Name = "Test2" } };
        var connection = Substitute.For<ISqliteConnection>();
        var expectedResult = new UpsertManyResult(0, 2, 0);
        _mockUpserter.UpsertMany(connection, entities).Returns(expectedResult);

        // Act
        var result = _entityServices.UpsertMany(connection, entities);

        // Assert
        Assert.That(result, Is.EqualTo(expectedResult));
        _mockUpserter.Received(1).UpsertMany(connection, entities);
    }
}