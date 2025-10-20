namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityGetter
{
    ISqliteQueryable<T> Get<T>(ISqliteConnection connection, bool recursiveLoad) where T : new();
}