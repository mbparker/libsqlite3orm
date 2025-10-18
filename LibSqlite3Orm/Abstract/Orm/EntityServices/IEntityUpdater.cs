using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityUpdater
{
    bool Update<T>(ISqliteConnection connection, T entity);
    bool Update<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity);
    int UpdateMany<T>(ISqliteConnection connection, IEnumerable<T> entities);
}