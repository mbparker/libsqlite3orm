using System.ComponentModel.DataAnnotations.Schema;

namespace LibSqlite3Orm.IntegrationTests.TestDataModel;

public class TestEntityTagLink
{
    public string Id { get; set; }
    public long TagId { get; set; }
    [NotMapped]
    public Lazy<TestEntityTag> Tag { get; set; }
    public long EntityId { get; set; }
    [NotMapped]
    public Lazy<TestEntityMaster> Entity { get; set; }
}