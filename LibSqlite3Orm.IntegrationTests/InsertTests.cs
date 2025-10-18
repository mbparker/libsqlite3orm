using LibSqlite3Orm.IntegrationTests.TestDataModel;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class InsertTests : IntegrationTestBase<TestDbContext>
{
    [Test]
    public void Insert_WhenMasterWithMaxFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithMaxValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }
    
    [Test]
    public void Insert_WhenMasterWithMinFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithMinValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    } 
    
    [Test]
    public void Insert_WhenMasterWithRandomFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }
    
    [Test]
    public void Insert_WhenTableHasNoUniqueColumnsOtherThanPK_AddingASecondTmeDoesNotThrow()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(2));
    }
    
    [Test]
    public void InsertMany_WhenMasterWithRandomFieldValues_RecordsStoredAccurately()
    {
        var entity1 = CreateTestEntityMasterWithRandomValues();
        var entity2 = CreateTestEntityMasterWithRandomValues();
        var entity3 = CreateTestEntityMasterWithRandomValues();
        TestEntityMaster[] entities = [entity1, entity2, entity3];
        
        Assert.That(Orm.InsertMany(entities), Is.EqualTo(3));
        Assert.That(entity1.Id, Is.EqualTo(1));
        Assert.That(entity2.Id, Is.EqualTo(2));
        Assert.That(entity3.Id, Is.EqualTo(3));

        var actual = Orm
            .Get<TestEntityMaster>()
            .AsEnumerable()
            .ToArray();
        
        Assert.That(actual.Length, Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++)
            AssertThatRecordsMatch(entities[i], actual[i]);
    }  
    
    [Test]
    public void Insert_WhenTagWithRandomFieldValues_RecordStoredAccurately()
    {
        var entity = CreateTestEntityTagWithRandomValues();
        
        Assert.That(Orm.Insert(entity), Is.True);
        Assert.That(entity.Id, Is.EqualTo(1));

        var actual = Orm
            .Get<TestEntityTag>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }
    
    [Test]
    public void Insert_WhenViolatesUniqueColumn_Throws()
    {
        var entity1 = CreateTestEntityTagWithRandomValues();
        Orm.Insert(entity1);
        
        var entity2 = CreateTestEntityTagWithRandomValues();
        entity2.TagValue = entity1.TagValue;
        
        var ex = Assert.Throws<SqliteException>(() => Orm.Insert(entity2));
        Assert.That(ex?.Message, Is.EqualTo("UNIQUE constraint failed: TestEntityTag.TagValue"));
    }
    
    [Test]
    public void InsertMany_WhenTagWithRandomFieldValues_RecordsStoredAccurately()
    {
        var entity1 = CreateTestEntityTagWithRandomValues();
        var entity2 = CreateTestEntityTagWithRandomValues();
        var entity3 = CreateTestEntityTagWithRandomValues();
        TestEntityTag[] entities = [entity1, entity2, entity3];
        
        Assert.That(Orm.InsertMany(entities), Is.EqualTo(3));
        Assert.That(entity1.Id, Is.EqualTo(1));
        Assert.That(entity2.Id, Is.EqualTo(2));
        Assert.That(entity3.Id, Is.EqualTo(3));

        var actual = Orm
            .Get<TestEntityTag>()
            .AsEnumerable()
            .ToArray();
        
        Assert.That(actual.Length, Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++)
            AssertThatRecordsMatch(entities[i], actual[i]);
    }  
    
    [Test]
    public void Insert_WhenTagLink_RecordStoredAccurately()
    {
        // Create tag entity
        var tagEntity = CreateTestEntityTagWithRandomValues();
        Orm.Insert(tagEntity);
        
        // Create the main entity
        var masterEntity = CreateTestEntityMasterWithRandomValues();
        Orm.Insert(masterEntity);
        
        // Create the link
        var linkEntity = new TestEntityTagLink { EntityId = masterEntity.Id, TagId = tagEntity.Id };
        Assert.That(Orm.Insert(linkEntity), Is.True);
        Assert.That(linkEntity.Id, Is.Not.Null);

        var actual = Orm
            .Get<TestEntityTagLink>(loadNavigationProps: true)
            .Where(x => x.Id == linkEntity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(linkEntity, actual);
        AssertThatRecordsMatch(masterEntity, actual.Entity.Value);
        AssertThatRecordsMatch(tagEntity, actual.Tag.Value);
    }
    
    [Test]
    public void Insert_WhenTagLinkDuplicate_Throws()
    {
        // Create tag entity
        var tagEntity = CreateTestEntityTagWithRandomValues();
        Orm.Insert(tagEntity);
        
        // Create the main entity
        var masterEntity = CreateTestEntityMasterWithRandomValues();
        Orm.Insert(masterEntity);
        
        // Create the link
        var linkEntity = new TestEntityTagLink { EntityId = masterEntity.Id, TagId = tagEntity.Id };
        Assert.That(Orm.Insert(linkEntity), Is.True);
        Assert.That(linkEntity.Id, Is.Not.Null);
        
        var linkEntity2 = new TestEntityTagLink { EntityId = masterEntity.Id, TagId = tagEntity.Id };
        var ex = Assert.Throws<SqliteException>(() => Orm.Insert(linkEntity2));
        Assert.That(ex?.Message, Is.EqualTo("UNIQUE constraint failed: TestEntityTagLink.TagId, TestEntityTagLink.EntityId"));
    }
}