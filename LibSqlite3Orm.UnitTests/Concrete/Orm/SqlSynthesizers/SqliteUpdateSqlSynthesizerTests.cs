using System.Runtime.Serialization;
using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.SqlSynthesizers;

[TestFixture]
public class SqliteUpdateSqlSynthesizerTests
{
    private SqliteUpdateSqlSynthesizer _synthesizer;
    private SqliteDbSchema _schema;
    private SqliteDbSchemaTable _testTable;

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
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
        _testTable.Columns.Add("CreatedDate", new SqliteDbSchemaTableColumn { Name = "CreatedDate", IsImmutable = true });
        _testTable.Columns.Add("IsActive", new SqliteDbSchemaTableColumn { Name = "IsActive" });

        _testTable.PrimaryKey = new SqliteDbSchemaTablePrimaryKeyColumn
        {
            FieldName = "Id"
        };

        _schema.Tables.Add("TestTable", _testTable);
        _synthesizer = new SqliteUpdateSqlSynthesizer(_schema);
    }

    [Test]
    public void Synthesize_WithValidEntityType_GeneratesCorrectUpdateSql()
    {
        // Arrange
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Update));
        Assert.That(result.SqlText, Is.Not.Null.And.Not.Empty);
        Assert.That(result.SqlText, Does.StartWith("UPDATE TestTable SET"));
        Assert.That(result.SqlText, Does.Contain("IsActive = :IsActive"));
        Assert.That(result.SqlText, Does.Contain("Name = :Name"));
        Assert.That(result.SqlText, Does.Not.Contain("CreatedDate = :CreatedDate")); // Should exclude immutable
        // Should exclude primary key from SET
        Assert.That(result.SqlText, Does.Not.Contain(",Id = :Id")); 
        Assert.That(result.SqlText, Does.Not.Contain(" Id = :Id")); 
        //
        Assert.That(result.SqlText, Does.Contain("WHERE (Id = :Id)"));
        Assert.That(result.Schema, Is.EqualTo(_schema));
        Assert.That(result.Table, Is.EqualTo(_testTable));
    }

    [Test]
    public void Synthesize_ExcludesImmutableColumns()
    {
        // Arrange
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SqlText, Does.Not.Contain("CreatedDate = :CreatedDate"));
        Assert.That(result.SqlText, Does.Contain("IsActive = :IsActive"));
        Assert.That(result.SqlText, Does.Contain("Name = :Name"));
    }

    [Test]
    public void Synthesize_WithCompositeKey_GeneratesCorrectWhereClause()
    {
        // Arrange
        _testTable.CompositePrimaryKeyFields = ["Id", "Name"];
        _testTable.PrimaryKey = null;
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SqlText, Does.Contain("WHERE (Id = :Id AND Name = :Name)"));
        Assert.That(result.SqlText, Does.Not.Contain("Id = :Id,")); // Should not be in SET clause
        Assert.That(result.SqlText, Does.Not.Contain("Name = :Name,")); // Should not be in SET clause
        Assert.That(result.SqlText, Does.Contain("IsActive = :IsActive"));
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
        Assert.DoesNotThrow(() => new SqliteUpdateSqlSynthesizer(_schema));
    }

    [Test]
    public void Synthesize_ResultContainsCorrectMetadata()
    {
        // Arrange
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Update));
        Assert.That(result.Schema, Is.SameAs(_schema));
        Assert.That(result.Table, Is.SameAs(_testTable));
        Assert.That(result.ExtractedParameters, Is.Not.Null);
    }

    [Test]
    public void Synthesize_WithOnlyImmutableNonKeyColumns_GeneratesEmptySetClause()
    {
        // Arrange
        _testTable.Columns.Clear();
        _testTable.Columns.Add("Id", new SqliteDbSchemaTableColumn { Name = "Id" }); // Primary key
        _testTable.Columns.Add("CreatedDate", new SqliteDbSchemaTableColumn { Name = "CreatedDate", IsImmutable = true });
        
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SqlText, Does.StartWith("UPDATE TestTable SET "));
        Assert.That(result.SqlText, Does.Contain("WHERE (Id = :Id)"));
        // With no updatable columns, the SET clause should be minimal
        Assert.That(result.SqlText, Does.Not.Contain("CreatedDate = :CreatedDate"));
    }

    [Test]
    public void Synthesize_ColumnsOrderedAlphabetically()
    {
        // Arrange  
        _testTable.Columns.Clear();
        _testTable.Columns.Add("Id", new SqliteDbSchemaTableColumn { Name = "Id" }); // Primary key
        _testTable.Columns.Add("ZColumn", new SqliteDbSchemaTableColumn { Name = "ZColumn" });
        _testTable.Columns.Add("AColumn", new SqliteDbSchemaTableColumn { Name = "AColumn" });
        _testTable.Columns.Add("MColumn", new SqliteDbSchemaTableColumn { Name = "MColumn" });
        
        var args = SqliteDmlSqlSynthesisArgs.Empty;

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        // Columns should be in alphabetical order in the SET clause
        var setIndex = result.SqlText.IndexOf("SET ");
        var whereIndex = result.SqlText.IndexOf(" WHERE ");
        var setClause = result.SqlText.Substring(setIndex + 4, whereIndex - setIndex - 4);
        
        Assert.That(setClause, Does.Contain("AColumn = :AColumn"));
        Assert.That(setClause, Does.Contain("MColumn = :MColumn"));  
        Assert.That(setClause, Does.Contain("ZColumn = :ZColumn"));
    }
}