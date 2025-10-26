using System.Linq.Expressions;
using System.Numerics;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteQueryable<T> : ISqliteEnumerable<T>
{
    long Count();
    long Count(Expression<Func<T, bool>> predicate);
    TValue Sum<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>;
    double Total<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>;
    TValue Min<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>;
    TValue Max<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>;
    double Average<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>;
    ISqliteQueryable<T> Where(Expression<Func<T, bool>> predicate);
    ISqliteOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr);
    ISqliteOrderedQueryable<T> OrderBy(Expression keySelectorExpr);
    ISqliteOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr);
    ISqliteOrderedQueryable<T> OrderByDescending(Expression keySelectorExpr);
}