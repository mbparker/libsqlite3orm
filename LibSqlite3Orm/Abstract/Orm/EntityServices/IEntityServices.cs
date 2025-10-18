namespace LibSqlite3Orm.Abstract.Orm.EntityServices;

public interface IEntityServices : IEntityCreator, IEntityGetter, IEntityUpdater, IEntityDeleter,
    IEntityUpserter
{
}