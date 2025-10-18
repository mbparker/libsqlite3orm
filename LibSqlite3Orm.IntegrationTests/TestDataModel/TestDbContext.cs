using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.IntegrationTests.TestDataModel;

public class TestDbContext : SqliteOrmDatabaseContext
{
    public TestDbContext(Func<SqliteDbSchemaBuilder> schemaBuilderFactory) 
        : base(schemaBuilderFactory)
    {
    }

    protected override void BuildSchema(SqliteDbSchemaBuilder builder)
    {
        var demoEntity = builder.HasTable<TestEntityMaster>();
        demoEntity.WithAllMembersAsColumns(x => x.Id).IsAutoIncrement();
        demoEntity.WithColumnChanges(x => x.StringValue).UsingCollation();
        
        var customTag = builder.HasTable<TestEntityTag>();
        customTag.WithAllMembersAsColumns(x => x.Id).IsAutoIncrement();
        customTag.WithColumnChanges(x => x.TagValue).UsingCollation().IsUnique().IsNotNull();
        
        var customTagLink = builder.HasTable<TestEntityTagLink>();
        customTagLink.WithAllMembersAsColumns(x => x.Id).IsAutoGuid();
        customTagLink
            .WithForeignKey(x => x.TagId)
            .References<TestEntityTag>(x => x.Id)
            .HasNavigationProperty(x => x.Tag)
            .OnDelete(SqliteForeignKeyAction.Cascade);
        customTagLink
            .WithForeignKey(x => x.EntityId)
            .References<TestEntityMaster>(x => x.Id)
            .HasNavigationProperty(x => x.Entity)
            .HasForeignNavigationProperty<TestEntityMaster>(x => x.Tags)
            .OnDelete(SqliteForeignKeyAction.Cascade);
        
        builder.HasIndex<TestEntityMaster>().WithColumn(x => x.StringValue).UsingCollation().SortedAscending();
        builder.HasIndex<TestEntityTag>().WithColumn(x => x.TagValue).UsingCollation().SortedAscending();
        builder.HasIndex<TestEntityTagLink>().WithColumn(x => x.EntityId).UsingCollation().SortedAscending();
        builder.HasIndex<TestEntityTagLink>().WithColumn(x => x.TagId).UsingCollation().SortedAscending();
        var idx = builder.HasIndex<TestEntityTagLink>().IsUnique();
        idx.WithColumn(x => x.TagId);
        idx.WithColumn(x => x.EntityId);
    }
}