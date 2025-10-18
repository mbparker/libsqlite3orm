using System.Linq.Expressions;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.UnitTests.Concrete.Orm.SqlSynthesizers;

[TestFixture]
public class SqliteDeleteSqlSynthesizerTestsFixed
{
    private SqliteDeleteSqlSynthesizer _synthesizer;
    private SqliteDbSchema _schema;
    private SqliteDbSchemaTable _testTable;
    private ISqliteWhereClauseBuilder _mockWhereClauseBuilder;
    private Func<SqliteDbSchema, ISqliteWhereClauseBuilder> _whereClauseBuilderFactory;

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

        _schema.Tables.Add("TestTable", _testTable);

        _mockWhereClauseBuilder = Substitute.For<ISqliteWhereClauseBuilder>();
        _whereClauseBuilderFactory = Substitute.For<Func<SqliteDbSchema, ISqliteWhereClauseBuilder>>();
        _whereClauseBuilderFactory.Invoke(_schema).Returns(_mockWhereClauseBuilder);

        _synthesizer = new SqliteDeleteSqlSynthesizer(_schema, _whereClauseBuilderFactory);
    }

    [Test]
    public void Synthesize_WithValidEntityTypeAndFilter_GeneratesCorrectDeleteSql()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> filterExpr = e => e.Id == 1;
        var deleteArgs = new SynthesizeDeleteSqlArgs(filterExpr);
        var args = new SqliteDmlSqlSynthesisArgs(deleteArgs);

        _mockWhereClauseBuilder.Build(typeof(TestEntity), filterExpr).Returns("Id = :Id");
        var extractedParams = new Dictionary<string, ExtractedParameter>
        {
            //{ "Id", new ExtractedParameter("Id", 1, "Id") }
        };
        _mockWhereClauseBuilder.ExtractedParameters.Returns(extractedParams);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Delete));
        Assert.That(result.SqlText, Is.EqualTo("DELETE FROM TestTable WHERE Id = :Id;"));
        Assert.That(result.Schema, Is.EqualTo(_schema));
        Assert.That(result.Table, Is.EqualTo(_testTable));
        Assert.That(result.ExtractedParameters, Is.EqualTo(extractedParams));
    }

    [Test]
    public void Synthesize_WithoutFilter_GeneratesDeleteAllSql()
    {
        // Arrange
        var deleteArgs = new SynthesizeDeleteSqlArgs(null);
        var args = new SqliteDmlSqlSynthesisArgs(deleteArgs);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Delete));
        Assert.That(result.SqlText, Is.EqualTo("DELETE FROM TestTable;"));
        Assert.That(result.ExtractedParameters, Is.Empty);
        _whereClauseBuilderFactory.DidNotReceive().Invoke(Arg.Any<SqliteDbSchema>());
    }

    [Test]
    public void Synthesize_WithComplexFilter_CallsWhereClauseBuilder()
    {
        // Arrange
        Expression<Func<TestEntity, bool>> filterExpr = e => e.Name.Contains("test") && e.Id > 10;
        var deleteArgs = new SynthesizeDeleteSqlArgs(filterExpr);
        var args = new SqliteDmlSqlSynthesisArgs(deleteArgs);

        _mockWhereClauseBuilder.Build(typeof(TestEntity), filterExpr).Returns("Name LIKE '%test%' AND Id > :Id");
        var extractedParams = new Dictionary<string, ExtractedParameter>
        {
            //{ "Id", new ExtractedParameter("Id", 10, "Id") }
        };
        _mockWhereClauseBuilder.ExtractedParameters.Returns(extractedParams);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SqlText, Is.EqualTo("DELETE FROM TestTable WHERE Name LIKE '%test%' AND Id > :Id;"));
        _whereClauseBuilderFactory.Received(1).Invoke(_schema);
        _mockWhereClauseBuilder.Received(1).Build(typeof(TestEntity), filterExpr);
        Assert.That(result.ExtractedParameters, Is.EqualTo(extractedParams));
    }

    [Test]
    public void Synthesize_WithUnmappedEntityType_ThrowsInvalidDataContractException()
    {
        // Arrange
        var deleteArgs = new SynthesizeDeleteSqlArgs(null);
        var args = new SqliteDmlSqlSynthesisArgs(deleteArgs);

        // Act & Assert
        Assert.Throws<InvalidDataContractException>(() => _synthesizer.Synthesize(typeof(string), args));
    }

    [Test]
    public void Constructor_WithValidDependencies_InitializesCorrectly()
    {
        // Act & Assert
        Assert.That(_synthesizer, Is.Not.Null);
        Assert.DoesNotThrow(() => new SqliteDeleteSqlSynthesizer(_schema, _whereClauseBuilderFactory));
    }

    [Test]
    public void Synthesize_ResultContainsCorrectMetadata()
    {
        // Arrange
        var deleteArgs = new SynthesizeDeleteSqlArgs(null);
        var args = new SqliteDmlSqlSynthesisArgs(deleteArgs);

        // Act
        var result = _synthesizer.Synthesize(typeof(TestEntity), args);

        // Assert
        Assert.That(result.SynthesisKind, Is.EqualTo(SqliteDmlSqlSynthesisKind.Delete));
        Assert.That(result.Schema, Is.SameAs(_schema));
        Assert.That(result.Table, Is.SameAs(_testTable));
    }
}