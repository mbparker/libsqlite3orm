using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.IntegrationTests.TestDataModel;

public class TestDbContextAddedTable : TestDbContext
{
    public TestDbContextAddedTable(Func<SqliteDbSchemaBuilder> schemaBuilderFactory) 
        : base(schemaBuilderFactory)
    {
    }

    protected override void BuildSchema(SqliteDbSchemaBuilder builder)
    {
        base.BuildSchema(builder);
        
        var newTable = builder.HasTable<TestEntityAdded>();
        newTable.WithPrimaryKey(x => x.Id).IsAutoIncrement();
        newTable.WithColumn(x => x.NewDataField);
    }
}