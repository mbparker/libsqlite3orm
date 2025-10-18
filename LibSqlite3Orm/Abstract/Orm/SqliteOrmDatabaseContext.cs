using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public abstract class SqliteOrmDatabaseContext : ISqliteOrmDatabaseContext
{
    private readonly Func<SqliteDbSchemaBuilder> schemaBuilderFactory;
    private SqliteDbSchema _schema;
    
    protected SqliteOrmDatabaseContext(Func<SqliteDbSchemaBuilder> schemaBuilderFactory)
    {
        this.schemaBuilderFactory = schemaBuilderFactory;
    }
    
    public SqliteDbSchema Schema
    {
        get
        {
            if (_schema is null)
                _schema = BuildSchema();
            return _schema;
        }
    }
    
    protected abstract void BuildSchema(SqliteDbSchemaBuilder builder);
    
    private SqliteDbSchema BuildSchema()
    {
        var builder = schemaBuilderFactory();
        BuildSchema(builder);
        return builder.Build();
    }
}