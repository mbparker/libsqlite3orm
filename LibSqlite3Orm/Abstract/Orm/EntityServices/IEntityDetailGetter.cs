namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityDetailGetter
{
    Lazy<TDetails> GetDetails<TEntity, TDetails>(TEntity record, bool recursiveLoad, ISqliteDataRow row,
        ISqliteConnection connection)
        where TDetails : new();

    Lazy<ISqliteQueryable<TDetails>> GetDetailsList<TEntity, TDetails>(TEntity record, bool recursiveLoad,
        ISqliteConnection connection)
        where TDetails : new();
} 