using System.Linq.Expressions;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteOrderedQueryable<T> : ISqliteEnumerable<T>
{
    ISqliteOrderedQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr);
    ISqliteOrderedQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr);
}