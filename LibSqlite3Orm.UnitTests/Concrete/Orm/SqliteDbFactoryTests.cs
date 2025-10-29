using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm;

[TestFixture]
public class SqliteDbFactoryTests
{
    private SqliteDbFactory _dbFactory;
    private ISqliteConnection _mockConnection;
    private ISqliteCommand _mockCommand;
    private ISqliteDdlSqlSynthesizer _mockSynthesizer;
    private Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> _synthesizerFactory;

    [SetUp]
    public void SetUp()
    {
        _mockConnection = Substitute.For<ISqliteConnection>();
        _mockConnection.Connected.Returns(true);
        _mockCommand = Substitute.For<ISqliteCommand>();
        _mockSynthesizer = Substitute.For<ISqliteDdlSqlSynthesizer>();
        
        _synthesizerFactory = Substitute.For<Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer>>();
        
        _mockConnection.CreateCommand().Returns(_mockCommand);
        _synthesizerFactory.Invoke(Arg.Any<SqliteDdlSqlSynthesisKind>(), Arg.Any<SqliteDbSchema>()).Returns(_mockSynthesizer);

        _dbFactory = new SqliteDbFactory(_synthesizerFactory);
    }

    [TearDown]
    public void TearDown()
    {
        // Dispose mocks to satisfy NUnit analyzer
        _mockConnection?.Dispose();
        _mockCommand?.Dispose();
    }

    [Test]
    public void Constructor_WithValidParameters_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() => new SqliteDbFactory(_synthesizerFactory));
    }

    [Test]
    public void Constructor_WithNullSynthesizerFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new SqliteDbFactory(null));
    }

    [Test]
    public void Create_WithValidParameters_CreatesCommands()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _mockConnection);

        // Assert
        _mockConnection.Received(2).CreateCommand();
    }

    [Test]
    public void Create_WithValidParameters_ExecutesSqlCommand()
    {
        // Arrange
        var schema = CreateTestSchema();
        var expectedSql = "CREATE TABLE test (id INTEGER);";
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns(expectedSql);

        // Act
        _dbFactory.Create(schema, _mockConnection);

        // Assert
        _mockCommand.Received(1).ExecuteNonQuery(Arg.Is<string>(sql => sql.Contains(expectedSql)));
    }

    [Test]
    public void Create_WithValidParameters_DisposesResources()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _mockConnection);

        // Assert
        _mockCommand.Received(2).Dispose();
    }

    [Test]
    public void Create_WithSchemaContainingMultipleTables_CallsSynthesizeCreateForEachTable()
    {
        // Arrange
        var schema = new SqliteDbSchema();
        var table1 = new SqliteDbSchemaTable { Name = "Table1" };
        var table2 = new SqliteDbSchemaTable { Name = "Table2" };
        schema.Tables.Add("Table1", table1);
        schema.Tables.Add("Table2", table2);

        _mockSynthesizer.SynthesizeCreate("Table1").Returns("CREATE TABLE Table1 (id INTEGER);");
        _mockSynthesizer.SynthesizeCreate("Table2").Returns("CREATE TABLE Table2 (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _mockConnection);

        // Assert
        _mockSynthesizer.Received(1).SynthesizeCreate("Table1");
        _mockSynthesizer.Received(1).SynthesizeCreate("Table2");
    }

    [Test]
    public void Create_WithSchemaContainingIndexes_CallsSynthesizeCreateForEachIndex()
    {
        // Arrange
        var schema = new SqliteDbSchema();
        var table = new SqliteDbSchemaTable { Name = "TestTable" };
        schema.Tables.Add("TestTable", table);
        schema.Indexes.Add("Index1", new SqliteDbSchemaIndex { IndexName = "Index1" });
        schema.Indexes.Add("Index2", new SqliteDbSchemaIndex { IndexName = "Index2" });

        _mockSynthesizer.SynthesizeCreate("TestTable").Returns("CREATE TABLE TestTable (id INTEGER);");
        
        // Set up index synthesizer
        var indexSynthesizer = Substitute.For<ISqliteDdlSqlSynthesizer>();
        _synthesizerFactory.Invoke(SqliteDdlSqlSynthesisKind.IndexOps, schema).Returns(indexSynthesizer);
        indexSynthesizer.SynthesizeCreate("Index1").Returns("CREATE INDEX Index1 ON TestTable (column);");
        indexSynthesizer.SynthesizeCreate("Index2").Returns("CREATE INDEX Index2 ON TestTable (column);");

        // Act
        _dbFactory.Create(schema, _mockConnection);

        // Assert
        indexSynthesizer.Received(1).SynthesizeCreate("Index1");
        indexSynthesizer.Received(1).SynthesizeCreate("Index2");
    }

    [Test]
    public void Create_WithNullSchema_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dbFactory.Create(null, _mockConnection));
    }

    [Test]
    public void Create_WithNullConnection_ThrowsArgumentNullException()
    {
        // Arrange
        var schema = CreateTestSchema();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _dbFactory.Create(schema, null));
    }

    [Test]
    public void Create_CallsSynthesizeFactoryWithCorrectKinds()
    {
        // Arrange
        var schema = CreateTestSchema();
        _mockSynthesizer.SynthesizeCreate(Arg.Any<string>()).Returns("CREATE TABLE test (id INTEGER);");

        // Act
        _dbFactory.Create(schema, _mockConnection);

        // Assert
        _synthesizerFactory.Received().Invoke(SqliteDdlSqlSynthesisKind.TableOps, schema);
    }

    private SqliteDbSchema CreateTestSchema()
    {
        var schema = new SqliteDbSchema();
        var table = new SqliteDbSchemaTable { Name = "TestTable" };
        schema.Tables.Add("TestTable", table);
        return schema;
    }
}