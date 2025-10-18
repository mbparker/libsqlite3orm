namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityGetter
{
    ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool loadNavigationProps) where T : new();
}