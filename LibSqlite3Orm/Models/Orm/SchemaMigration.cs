namespace LibSqlite3Orm.Models.Orm;

public class SchemaMigration
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Schema { get; set; }
}