namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityDetailGetter
{
    Lazy<TDetails> GetDetails<TEntity, TDetails>(TEntity record, bool loadNavigationProps, ISqliteDataRow row,
        ISqliteConnection connection)
        where TDetails : new();

    Lazy<ISqliteQueryable<TDetails>> GetDetailsList<TEntity, TDetails>(TEntity record, bool loadNavigationProps,
        ISqliteConnection connection)
        where TDetails : new();
} 