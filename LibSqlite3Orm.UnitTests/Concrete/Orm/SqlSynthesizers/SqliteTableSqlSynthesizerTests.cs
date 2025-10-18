using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.PInvoke.Types.Enums;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.SqlSynthesizers;

[TestFixture]
public class SqliteTableSqlSynthesizerTests
{
    private SqliteTableSqlSynthesizer _synthesizer;
    private SqliteDbSchema _schema;
    private SqliteDbSchemaTable _testTable;

    [SetUp]
    public void SetUp()
    {
        _schema = new SqliteDbSchema();
        _testTable = new SqliteDbSchemaTable
        {
            Name = "TestTable"
        };

        _testTable.Columns.Add("Id", new SqliteDbSchemaTableColumn 
        { 
            Name = "Id", 
            DbFieldTypeAffinity = SqliteDataType.Integer,
            IsNotNull = true
        });
        _testTable.Columns.Add("Name", new SqliteDbSchemaTableColumn 
        { 
            Name = "Name", 
            DbFieldTypeAffinity = SqliteDataType.Text
        });

        _testTable.PrimaryKey = new SqliteDbSchemaTablePrimaryKeyColumn
        {
            FieldName = "Id",
            Ascending = true,
            AutoIncrement = true
        };

        _schema.Tables.Add("TestTable", _testTable);
        _synthesizer = new SqliteTableSqlSynthesizer(_schema);
    }

    [Test]
    public void SynthesizeCreate_WithBasicTable_GeneratesCorrectCreateTableSql()
    {
        // Act
        var result = _synthesizer.SynthesizeCreate("TestTable");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.StartWith("CREATE TABLE IF NOT EXISTS TestTable"));
        Assert.That(result, Does.Contain("Id INTEGER NOT NULL PRIMARY KEY ASC AUTOINCREMENT"));
        Assert.That(result, Does.Contain("Name TEXT"));
        Assert.That(result, Does.EndWith(");"));
    }

    [Test]
    public void SynthesizeCreate_WithNewObjectName_UsesNewName()
    {
        // Act
        var result = _synthesizer.SynthesizeCreate("TestTable", "NewTableName");

        // Assert
        Assert.That(result, Does.StartWith("CREATE TABLE IF NOT EXISTS NewTableName"));
    }

    [Test]
    public void SynthesizeCreate_WithUniqueColumn_IncludesUniqueConstraint()
    {
        // Arrange
        _testTable.Columns["Name"].IsUnique = true;

        // Act
        var result = _synthesizer.SynthesizeCreate("TestTable");

        // Assert
        Assert.That(result, Does.Contain("Name TEXT UNIQUE"));
    }

    [Test]
    public void Constructor_WithValidSchema_InitializesCorrectly()
    {
        // Act & Assert
        Assert.That(_synthesizer, Is.Not.Null);
        Assert.DoesNotThrow(() => new SqliteTableSqlSynthesizer(_schema));
    }

    [Test]
    public void SynthesizeCreate_WithNonExistentTable_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _synthesizer.SynthesizeCreate("NonExistentTable"));
    }

    [Test]
    public void SynthesizeCreate_MultipleCallsWithSameTable_ReturnConsistentResults()
    {
        // Act
        var result1 = _synthesizer.SynthesizeCreate("TestTable");
        var result2 = _synthesizer.SynthesizeCreate("TestTable");

        // Assert
        Assert.That(result1, Is.EqualTo(result2));
    }
}