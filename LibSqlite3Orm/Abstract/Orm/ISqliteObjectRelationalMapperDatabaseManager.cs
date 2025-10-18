using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteObjectRelationalMapperDatabaseManager<TContext> : IDisposable where TContext : ISqliteOrmDatabaseContext
{
    SqliteDbSchemaChanges DetectedSchemaChanges { get; }
    
    void UseConnection(ISqliteConnection connection);
    
    void CreateDatabase();
    bool Migrate();
    void DeleteDatabase();
}