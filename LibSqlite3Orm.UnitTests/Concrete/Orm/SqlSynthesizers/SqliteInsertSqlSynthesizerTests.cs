using System.Runtime.Serialization;
using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.SqlSynthesizers;

[TestFixture]
public class SqliteInsertSqlSynthesizerTests
{
    private SqliteInsertSqlSynthesizer _synthesizer;
    private SqliteDbSchema _schema;
    private SqliteDbSchemaTable _testTable;

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    [SetUp]
    public void SetUp()
    {
        _schema = new SqliteDbSchema();
        _testTable = new SqliteDbSchemaTable
        {
            Name = "TestTable",
            ModelTypeName = typeof(TestEntity).AssemblyQualifiedName
        };

        _testTable.Columns.Add("Id", new SqliteDbSchemaTableColumn { Name = "Id" });
        _testTable.Columns.Add("Name", new SqliteDbSchemaTableColumn { Name = "Name" });
        _testTable.Columns.Add("CreatedDate", new SqliteDbSchemaTableColumn { Name = "CreatedDate" });

        _testTable.PrimaryKey = new SqliteDbSchemaTablePrimaryKeyColumn
        {
            FieldName = "Id",
            AutoIncrement = false
        };

        _schema.Tables.Add("TestTable", _testTable);
        _synthesizer = new SqliteInsertSqlSynthesizer(_schema);
    }

    [Test]
    public void Synthesize_WithValidEntityType_GeneratesCorrectInsertSql()
    {
        // Arrange
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Insert));
        Assert.That(result.SqlText, Is.Not.Null.And.Not.Empty);
        Assert.That(result.SqlText, Does.StartWith("INSERT INTO TestTable"));
        Assert.That(result.SqlText, Does.Contain("(CreatedDate, Id, Name)"));
        Assert.That(result.SqlText, Does.Contain("VALUES (:CreatedDate, :Id, :Name)"));
        Assert.That(result.Schema, Is.EqualTo(_schema));
        Assert.That(result.Table, Is.EqualTo(_testTable));
    }

    [Test]
    public void Synthesize_WithAutoIncrementPrimaryKey_ExcludesPrimaryKeyFromInsert()
    {
        // Arrange
        _testTable.PrimaryKey.AutoIncrement = true;
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SqlText, Does.Not.Contain(":Id"));
        Assert.That(result.SqlText, Does.Not.Contain("Id,"));
        Assert.That(result.SqlText, Does.Contain("(CreatedDate, Name)"));
        Assert.That(result.SqlText, Does.Contain("VALUES (:CreatedDate, :Name)"));
    }

    [Test]
    public void Synthesize_WithUnmappedEntityType_ThrowsInvalidDataContractException()
    {
        // Arrange
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act & Assert
        Assert.Throws<InvalidDataContractException>(() => _synthesizer.Synthesize(typeof(string), args));
    }

    [Test]
    public void Constructor_WithValidSchema_InitializesCorrectly()
    {
        // Act & Assert
        Assert.That(_synthesizer, Is.Not.Null);
        Assert.DoesNotThrow(() => new SqliteInsertSqlSynthesizer(_schema));
    }

    [Test]
    public void Synthesize_ResultContainsCorrectMetadata()
    {
        // Arrange
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Insert));
        Assert.That(result.Schema, Is.SameAs(_schema));
        Assert.That(result.Table, Is.SameAs(_testTable));
        Assert.That(result.ExtractedParameters, Is.Not.Null);
    }
}