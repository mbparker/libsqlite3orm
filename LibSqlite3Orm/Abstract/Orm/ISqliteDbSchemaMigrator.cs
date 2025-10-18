using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteDbSchemaMigrator<TContext> : IDisposable where TContext : ISqliteOrmDatabaseContext
{
    void UseConnection(ISqliteConnection connection);
    
    void CreateInitialMigration();
    SqliteDbSchemaChanges CheckForSchemaChanges();
    void Migrate(SqliteDbSchemaChanges changes);
}