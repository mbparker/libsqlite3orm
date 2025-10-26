using LibSqlite3Orm.Models.Orm.OData;

namespace LibSqlite3Orm.Abstract.Orm.OData;

public interface IODataQueryHandler
{
    ODataQueryResult<TEntity> ODataQuery<TEntity>(ISqliteConnection connection, string odataQuery)
        where TEntity : new();
}