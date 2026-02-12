using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.SqlSynthesizers;

[TestFixture]
public class SqliteIndexSqlSynthesizerTests
{
    private SqliteIndexSqlSynthesizer _synthesizer;
    private SqliteDbSchema _schema;
    private SqliteDbSchemaIndex _testIndex;

    [SetUp]
    public void SetUp()
    {
        _schema = new SqliteDbSchema();
        _testIndex = new SqliteDbSchemaIndex
        {
            IndexName = "IX_TestTable_Name",
            TableName = "TestTable",
            IsUnique = false
        };

        _testIndex.Columns.Add(new SqliteDbSchemaIndexColumn
        {
            Name = "Name",
            SortDescending = false
        });

        _schema.Indexes.Add("IX_TestTable_Name", _testIndex);
        _synthesizer = new SqliteIndexSqlSynthesizer(_schema);
    }

    [Test]
    public void SynthesizeCreate_WithBasicIndex_GeneratesCorrectCreateIndexSql()
    {
        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("CREATE INDEX IF NOT EXISTS IX_TestTable_Name ON TestTable (Name ASC);"));
    }

    [Test]
    public void SynthesizeCreate_WithUniqueIndex_GeneratesUniqueIndexSql()
    {
        // Arrange
        _testIndex.IsUnique = true;

        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result, Is.EqualTo("CREATE UNIQUE INDEX IF NOT EXISTS IX_TestTable_Name ON TestTable (Name ASC);"));
    }

    [Test]
    public void SynthesizeCreate_WithDescendingColumn_GeneratesDescendingSql()
    {
        // Arrange
        _testIndex.Columns[0].SortDescending = true;

        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result, Is.EqualTo("CREATE INDEX IF NOT EXISTS IX_TestTable_Name ON TestTable (Name DESC);"));
    }

    [Test]
    public void SynthesizeCreate_WithMultipleColumns_GeneratesMultiColumnIndex()
    {
        // Arrange
        _testIndex.Columns.Add(new SqliteDbSchemaIndexColumn
        {
            Name = "CreatedDate",
            SortDescending = true
        });

        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result, Is.EqualTo("CREATE INDEX IF NOT EXISTS IX_TestTable_Name ON TestTable (Name ASC, CreatedDate DESC);"));
    }

    [Test]
    public void SynthesizeCreate_WithNewObjectName_UsesNewName()
    {
        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name", "IX_NewIndexName");

        // Assert
        Assert.That(result, Does.StartWith("CREATE INDEX IF NOT EXISTS IX_NewIndexName ON"));
    }

    [Test]
    public void SynthesizeCreate_WithCollation_IncludesCollationInSql()
    {
        // Arrange
        _testIndex.Columns[0].Collation = SqliteCollation.AsciiLowercase;

        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result, Does.Contain("Name COLLATE NOCASE ASC"));
    }
    
    [Test]
    public void SynthesizeCreate_WithCustomCollation_IncludesCollationInSql()
    {
        // Arrange
        _testIndex.Columns[0].CustomCollation = "TEST_COLLATION";

        // Act
        var result = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result, Does.Contain("Name COLLATE TEST_COLLATION ASC"));
    }    

    [Test]
    public void SynthesizeDrop_WithIndexName_GeneratesDropIndexSql()
    {
        // Act
        var result = _synthesizer.SynthesizeDrop("IX_TestTable_Name");

        // Assert
        Assert.That(result, Is.EqualTo("DROP INDEX IF EXISTS IX_TestTable_Name;"));
    }

    [Test]
    public void Constructor_WithValidSchema_InitializesCorrectly()
    {
        // Act & Assert
        Assert.That(_synthesizer, Is.Not.Null);
        Assert.DoesNotThrow(() => new SqliteIndexSqlSynthesizer(_schema));
    }

    [Test]
    public void SynthesizeCreate_WithNonExistentIndex_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => _synthesizer.SynthesizeCreate("NonExistentIndex"));
    }

    [Test]
    public void SynthesizeCreate_MultipleCallsWithSameIndex_ReturnConsistentResults()
    {
        // Act
        var result1 = _synthesizer.SynthesizeCreate("IX_TestTable_Name");
        var result2 = _synthesizer.SynthesizeCreate("IX_TestTable_Name");

        // Assert
        Assert.That(result1, Is.EqualTo(result2));
    }
}