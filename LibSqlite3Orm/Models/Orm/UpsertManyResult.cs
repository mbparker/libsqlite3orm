namespace LibSqlite3Orm.Models.Orm;

public class UpsertManyResult
{
    public UpsertManyResult(int updateCount, int  insertCount, int failedCount)
    {
        UpdateCount = updateCount;
        InsertCount = insertCount;
        FailedCount = failedCount;
    }
    
    public int UpdateCount { get; }
    public int InsertCount { get; }
    public int FailedCount { get; }
}