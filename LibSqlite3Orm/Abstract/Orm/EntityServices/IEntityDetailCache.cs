using System.Linq.Expressions;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityDetailCache
{
    object TryGet(object masterEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp);
    void Upsert(object masterEntity, object detailEntity, SqliteDbSchemaTableForeignKeyNavigationProperty navProp);
    void Remove(object detailEntity);
    void Remove<T>(Expression<Func<T, bool>> predicate);
    void Clear();
}