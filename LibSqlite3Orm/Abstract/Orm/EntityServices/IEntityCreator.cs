using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityCreator
{
    bool Insert<T>(ISqliteConnection connection, T entity);
    bool Insert<T>(ISqliteConnection connection, DmlSqlSynthesisResult synthesisResult, T entity);
    int InsertMany<T>(ISqliteConnection connection, IEnumerable<T> entities);
}