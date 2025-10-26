using Newtonsoft.Json;

namespace LibSqlite3Orm.Models.Orm.OData;

public class ODataQueryResult<TEntity> where TEntity : new()
{
    [JsonConstructor]
    public ODataQueryResult()
    {
    }
    
    public ODataQueryResult(IEnumerable<TEntity> entities, long? count)
    {
        Entities = entities;
        Count = count;
    }    
    
    public IEnumerable<TEntity> Entities { get; set; } = [];
    public long? Count { get; set; }
}