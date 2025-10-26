using System.Collections;
using System.Linq.Expressions;
using System.Numerics;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

// This is basically just a data collector for deferred SQL execution and results enumeration.
// We can't generate the SQL until we know filtering and sorting - which get invoked on the result of the ORM Get call.
// Therefore, we can't actually query until we have that information. So the trigger becomes the invocation of GetEnumerator.
public class SqliteOrderedQueryable<T> : ISqliteQueryable<T>, ISqliteOrderedQueryable<T>, IEnumerable<T>
{
    private readonly SqliteDbSchema schema;
    private readonly Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc;
    private readonly Func<ISqliteDataRow, T> modelDeserializerFunc;
    private readonly List<SqliteSortSpec> sortSpecs;
    private readonly bool recursiveLoad;
    private Expression<Func<T, bool>> wherePredicate;
    private int? skipCount;
    private int? takeCount;

    public SqliteOrderedQueryable(SqliteDbSchema schema,
        Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc,
        Func<ISqliteDataRow, T> modelDeserializerFunc, bool recursiveLoad)
        : this(schema, executeFunc, modelDeserializerFunc, recursiveLoad, null, null, null, null)
    {
    }

    private SqliteOrderedQueryable(SqliteDbSchema schema,
        Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc,
        Func<ISqliteDataRow, T> modelDeserializerFunc, bool recursiveLoad, Expression<Func<T, bool>> wherePredicate, SqliteSortSpec newSpec,
        int? skipCount, int? takeCount)
        : this(schema, executeFunc, modelDeserializerFunc, recursiveLoad, wherePredicate, [], skipCount, takeCount, newSpec)
    {
    }

    private SqliteOrderedQueryable(SqliteDbSchema schema,
        Func<SynthesizeSelectSqlArgs, ISqliteDataReader> executeFunc,
        Func<ISqliteDataRow, T> modelDeserializerFunc, bool recursiveLoad, Expression<Func<T, bool>> wherePredicate,
        List<SqliteSortSpec> sortSpecs, int? skipCount, int? takeCount, SqliteSortSpec newSpec)
    {
        this.schema = schema;
        this.executeFunc = executeFunc;
        this.modelDeserializerFunc = modelDeserializerFunc;
        this.recursiveLoad = recursiveLoad;
        this.wherePredicate = wherePredicate;
        this.sortSpecs = sortSpecs;
        this.skipCount = skipCount;
        this.takeCount = takeCount;        
        if (newSpec is not null)
            this.sortSpecs.Add(newSpec);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new SqliteOrderedEnumerator(
            executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                wherePredicate, sortSpecs.ToArray(), skipCount, takeCount, aggFunc: null, null)), modelDeserializerFunc);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T SingleRecord()
    {
        return this.SingleOrDefault();
    }

    public T[] AllRecords()
    {
        return AsEnumerable().ToArray();
    }

    public IEnumerable<T> AsEnumerable()
    {
        return this;
    }

    public ISqliteEnumerable<T> Skip(int count)
    {
        skipCount = count;
        return this;
    }

    public ISqliteEnumerable<T> Take(int count)
    {
        takeCount = count;
        return this;
    }

    public long Count()
    {
        return Count(null);
    }

    public long Count(Expression<Func<T, bool>> predicate)
    {
        Expression<Func<T, bool>> effectivePredicate = null;
        if (predicate is not null)
        {
            if (wherePredicate is not null)
                effectivePredicate = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(wherePredicate.Body, predicate.Body),
                    predicate.Parameters);
            else
                effectivePredicate = predicate;
        }

        using (var dataReader = executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                   effectivePredicate, sortSpecs.ToArray(), skipCount, takeCount, SqliteAggregateFunction.Count, null)))
        {
            return dataReader.First()[0].ValueAs<long>();
        }
    }

    public TValue Sum<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>
    {
        if (valueSelector.Body is MemberExpression me)
        {
            using (var dataReader = executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                       wherePredicate, sortSpecs.ToArray(), skipCount, takeCount, SqliteAggregateFunction.Sum, me.Member)))
            {
                return dataReader.First()[0].ValueAs<TValue>();
            }
        }
        
        return default;
    }
    
    public double Total<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>
    {
        if (valueSelector.Body is MemberExpression me)
        {
            using (var dataReader = executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                       wherePredicate, sortSpecs.ToArray(), skipCount, takeCount, SqliteAggregateFunction.Total, me.Member)))
            {
                return dataReader.First()[0].ValueAs<double>();
            }
        }
        
        return 0;
    }
    
    public TValue Min<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>
    {
        if (valueSelector.Body is MemberExpression me)
        {
            using (var dataReader = executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                       wherePredicate, sortSpecs.ToArray(), skipCount, takeCount, SqliteAggregateFunction.Min, me.Member)))
            {
                return dataReader.First()[0].ValueAs<TValue>();
            }
        }
        
        return default;
    }
    
    public TValue Max<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>
    {
        if (valueSelector.Body is MemberExpression me)
        {
            using (var dataReader = executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                       wherePredicate, sortSpecs.ToArray(), skipCount, takeCount, SqliteAggregateFunction.Max, me.Member)))
            {
                return dataReader.First()[0].ValueAs<TValue>();
            }
        }
        
        return default;
    }
    
    public double Average<TValue>(Expression<Func<T, TValue>> valueSelector) where TValue : INumber<TValue>
    {
        if (valueSelector.Body is MemberExpression me)
        {
            using (var dataReader = executeFunc.Invoke(new SynthesizeSelectSqlArgs(recursiveLoad,
                       wherePredicate, sortSpecs.ToArray(), skipCount, takeCount, SqliteAggregateFunction.Avg, me.Member)))
            {
                return dataReader.First()[0].ValueAs<double>();
            }
        }
        
        return 0;
    }

    public ISqliteQueryable<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (wherePredicate is not null)
            wherePredicate = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(wherePredicate.Body, predicate.Body),
                predicate.Parameters);
        else
            wherePredicate = predicate;
        return this;
    }

    public ISqliteOrderedQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: false);
    }
    
    public ISqliteOrderedQueryable<T> OrderBy(Expression keySelectorExpr)
    {
        return New(keySelectorExpr, descending: false);
    }    

    public ISqliteOrderedQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: true);
    }
    
    public ISqliteOrderedQueryable<T> OrderByDescending(Expression keySelectorExpr)
    {
        return New(keySelectorExpr, descending: true);
    }    

    public ISqliteOrderedQueryable<T> ThenBy<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: false);
    }
    
    public ISqliteOrderedQueryable<T> ThenBy(Expression keySelectorExpr)
    {
        return New(keySelectorExpr, descending: false);
    }    

    public ISqliteOrderedQueryable<T> ThenByDescending<TKey>(Expression<Func<T, TKey>> keySelectorExpr)
    {
        return New(keySelectorExpr, descending: true);
    }
    
    public ISqliteOrderedQueryable<T> ThenByDescending(Expression keySelectorExpr)
    {
        return New(keySelectorExpr, descending: true);
    }    

    private ISqliteOrderedQueryable<T> New(Expression keySelectorExpr, bool descending)
    {
        return new SqliteOrderedQueryable<T>(schema, executeFunc, modelDeserializerFunc, recursiveLoad,
            wherePredicate, sortSpecs, skipCount, takeCount, new SqliteSortSpec(schema, keySelectorExpr, descending));
    }

    private class SqliteOrderedEnumerator : IEnumerator<T>
    {
        private readonly Func<ISqliteDataRow, T> modelDeserializerFunc;
        private ISqliteDataReader dataReader;
        private IEnumerator<ISqliteDataRow> enumerator;
        private T current;
        private bool disposed;

        internal SqliteOrderedEnumerator(ISqliteDataReader dataReader, Func<ISqliteDataRow, T> modelDeserializerFunc)
        {
            this.dataReader = dataReader;
            this.modelDeserializerFunc = modelDeserializerFunc;
        }

        public bool MoveNext()
        {
            enumerator ??= dataReader.GetEnumerator();
            var result = enumerator.MoveNext();
            if (result)
                current = modelDeserializerFunc.Invoke(enumerator.Current);
            else
                current = default;
            return result;
        }

        public void Reset()
        {
            enumerator.Reset();
        }

        T IEnumerator<T>.Current => current;

        object IEnumerator.Current => current;

        public void Dispose()
        {
            if (!disposed)
            {
                current = default;
                enumerator.Dispose();
                enumerator = null;
                dataReader.Dispose();
                dataReader = null;
                disposed = true;
            }
        }
    }
}