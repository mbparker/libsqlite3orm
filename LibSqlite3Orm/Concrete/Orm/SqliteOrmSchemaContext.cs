using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteOrmSchemaContext : SqliteOrmDatabaseContext
{
    public SqliteOrmSchemaContext(Func<SqliteDbSchemaBuilder> schemaBuilderFactory)
        : base(schemaBuilderFactory)
    {
    }

    protected override void BuildSchema(SqliteDbSchemaBuilder builder)
    {
        var tab = builder.HasTable<SchemaMigration>("_ORM_schema_migrations");
        tab.WithPrimaryKey(x => x.Id).IsAutoIncrement();
        tab.WithColumn(x => x.Timestamp).IsNotNull();
        tab.WithColumn(x => x.Schema).IsNotNull();     
    }
}