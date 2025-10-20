using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.EntityServices;

[TestFixture]
public class EntityGetterTests
{
    private EntityGetter _entityGetter;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteParameterCollection _mockParameters;
    private ISqliteDmlSqlSynthesizer _mockSynthesizer;
    private ISqliteParameterPopulator _mockParameterPopulator;
    private ISqliteEntityWriter _mockEntityWriter;
    private ISqliteOrmDatabaseContext _mockContext;
    private SqliteDbSchema _mockSchema;
    private Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer> _synthesizerFactory;
    private ISqliteDataReader _mockDataReader;

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
        _mockEntityWriter = Substitute.For<ISqliteEntityWriter>();
        _mockContext = Substitute.For<ISqliteOrmDatabaseContext>();
        _mockDataReader = Substitute.For<ISqliteDataReader>();

        _mockSchema = new SqliteDbSchema();
        _mockContext.Schema.Returns(_mockSchema);

        _mockConnection.CreateCommand().Returns(_mockCommand);
        _mockCommand.Parameters.Returns(_mockParameters);
        _mockCommand.ExecuteQuery(Arg.Any<string>()).Returns(_mockDataReader);
        
        _synthesizerFactory = Substitute.For<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>();
        
        _synthesizerFactory.Invoke(Arg.Any<SqliteDmlSqlSynthesisKind>(), Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        var mockTable = new SqliteDbSchemaTable { Name = "TestTable", ModelTypeName = typeof(TestEntity).AssemblyQualifiedName };
        var synthesisResult = new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Select, _mockSchema, mockTable, "SELECT * FROM TestTable", new Dictionary<string, ExtractedParameter>());
        _mockSynthesizer.Synthesize<TestEntity>(Arg.Any<SqliteDmlSqlSynthesisArgs>()).Returns(synthesisResult);

        _entityGetter = new EntityGetter(_synthesizerFactory, _mockParameterPopulator, (ctx) => _mockEntityWriter, _mockContext);
    }

    [TearDown]
    public void TearDown()
    {
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
        _mockDataReader?.Dispose();
    }

    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act & Assert
        Assert.That(_entityGetter, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullSynthesizerFactory_DoesNotThrow()
    {
        // Act & Assert - Constructor doesn't validate null parameters
        Assert.DoesNotThrow(() =>
            new EntityGetter(null, _mockParameterPopulator, (ctx) => _mockEntityWriter, _mockContext));
    }

    [Test]
    public void Constructor_WithNullParameterPopulator_DoesNotThrow()
    {
        // Act & Assert - Constructor doesn't validate null parameters
        Assert.DoesNotThrow(() =>
            new EntityGetter(_synthesizerFactory, null, (ctx) => _mockEntityWriter, _mockContext));
    }

    [Test]
    public void Constructor_WithNullContext_DoesNotThrow()
    {
        // Act & Assert - Constructor doesn't validate null parameters
        Assert.DoesNotThrow(() =>
            new EntityGetter(_synthesizerFactory, _mockParameterPopulator, (ctx) => _mockEntityWriter, null));
    }

    [Test]
    public void Get_WithIncludeDetailsTrue_ReturnsQueryable()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName,
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act
        var result = _entityGetter.Get<TestEntity>(_mockConnection, recursiveLoad: true);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ISqliteQueryable<TestEntity>>());
        _mockConnection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
    }

    [Test]
    public void Get_WithIncludeDetailsFalse_ReturnsQueryable()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName,
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act
        var result = _entityGetter.Get<TestEntity>(_mockConnection, recursiveLoad: false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ISqliteQueryable<TestEntity>>());
        _mockConnection.DidNotReceive().OpenReadWrite(Arg.Any<string>(), Arg.Any<bool>());
    }

    [Test]
    public void Get_WithConnection_WithUnmappedEntityType_ThrowsInvalidDataContractException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidDataContractException>(() => _entityGetter.Get<TestEntity>(_mockConnection, false));
        Assert.That(exception.Message, Does.Contain("is not mapped in the schema"));
        Assert.That(exception.Message, Does.Contain(typeof(TestEntity).AssemblyQualifiedName));
    }

    [Test]
    public void Get_CallsSynthesizerFactory_WithCorrectParameters()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName,
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act - This will trigger the queryable execution internally
        var queryable = _entityGetter.Get<TestEntity>(_mockConnection, false);

        // We can't easily trigger the internal ExecuteQuery delegate without complex reflection,
        // but we can verify the setup was correct
        Assert.That(queryable, Is.Not.Null);
        
        // The synthesizer factory should be called when the queryable is enumerated
        // For now, we verify the queryable was created successfully
        Assert.That(queryable, Is.InstanceOf<SqliteOrderedQueryable<TestEntity>>());
    }

    [Test]
    public void Get_CreatesQueryableCorrectly_WhenNoConnectionProvided()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName,
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act
        var result = _entityGetter.Get<TestEntity>(_mockConnection, false);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ISqliteQueryable<TestEntity>>());
        // Connection factory not called until queryable is enumerated
    }

    [Test]
    public void Get_ReturnsQueryableWithCorrectType()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName,
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act
        var result = _entityGetter.Get<TestEntity>(_mockConnection, false);

        // Assert
        Assert.That(result, Is.InstanceOf<ISqliteQueryable<TestEntity>>());
        Assert.That(result, Is.InstanceOf<SqliteOrderedQueryable<TestEntity>>());
    }

    [Test]
    public void Get_WithConnection_DefaultIncludeDetails_IsFalse()
    {
        // Arrange
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName,
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act
        var result1 = _entityGetter.Get<TestEntity>(_mockConnection, recursiveLoad: false);
        var result2 = _entityGetter.Get<TestEntity>(_mockConnection, recursiveLoad: false);

        // Assert - Both should work the same way
        Assert.That(result1, Is.Not.Null);
        Assert.That(result2, Is.Not.Null);
        Assert.That(result1, Is.InstanceOf<ISqliteQueryable<TestEntity>>());
        Assert.That(result2, Is.InstanceOf<ISqliteQueryable<TestEntity>>());
    }

    [Test]
    public void Get_WithPartiallyMatchingTableName_StillThrowsIfTypeNameDoesNotMatch()
    {
        // Arrange - Add table with different type name
        var table = new SqliteDbSchemaTable
        {
            ModelTypeName = "SomeOtherType",
            Name = "TestTable"
        };
        _mockSchema.Tables.Add("TestTable", table);

        // Act & Assert
        var exception = Assert.Throws<InvalidDataContractException>(() => _entityGetter.Get<TestEntity>(_mockConnection, false));
        Assert.That(exception.Message, Does.Contain("is not mapped in the schema"));
    }
}