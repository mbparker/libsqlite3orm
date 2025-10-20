using LibSqlite3Orm.IntegrationTests.TestDataModel;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class UpsertTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void Upsert_WhenExistingMasterValuesChange_TheyAreUpdatedAccurately()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        entity.Id = SeededMasterRecords[1].Id;
        
        var ret = Orm.Upsert(entity);
        Assert.That(ret, Is.EqualTo(UpsertResult.Updated));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(actual, entity);
    }
    
    [Test]
    public void Upsert_WhenNewMasterRecord_IsInsertedAccurately()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        
        var ret = Orm.Upsert(entity);
        Assert.That(ret, Is.EqualTo(UpsertResult.Inserted));
        Assert.That(entity.Id, Is.EqualTo(SeededMasterRecords.Count + 1));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(actual, entity);
    }
    
    [Test]
    public void Upsert_WhenExistingTagValuesChange_TheyAreUpdatedAccurately()
    {
        var entity = CreateTestEntityTagWithRandomValues();
        entity.Id = SeededTagRecords[1].Id;
        
        var ret = Orm.Upsert(entity);
        Assert.That(ret, Is.EqualTo(UpsertResult.Updated));
        
        var actual = Orm
            .Get<TestEntityTag>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(actual, entity);
    }
    
    [Test]
    public void Upsert_WhenNewTagRecord_IsInsertedAccurately()
    {
        var entity = CreateTestEntityTagWithRandomValues();
        
        var ret = Orm.Upsert(entity);
        Assert.That(ret, Is.EqualTo(UpsertResult.Inserted));
        Assert.That(entity.Id, Is.EqualTo(SeededTagRecords.Count + 1));

        var actual = Orm
            .Get<TestEntityTag>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(actual, entity);
    }
    
    [Test]
    public void Upsert_WhenExistingLinkedTagIdChanges_ItIsUpdatedAccurately()
    {
        var linkEntity = SeededLinkRecords.Values.First();
        var usedTagIds = SeededLinkRecords.Where(x => x.Value.EntityId == linkEntity.EntityId)
            .Select(x => x.Value.TagId).ToArray();
        var availableTagId = SeededTagRecords.Keys.First(x => !usedTagIds.Contains(x));
        
        linkEntity.TagId = availableTagId;
        var ret = Orm.Upsert(linkEntity);
        Assert.That(ret, Is.EqualTo(UpsertResult.Updated));
        
        var actual = Orm
            .Get<TestEntityTagLink>(recursiveLoad: true)
            .Where(x => x.Id == linkEntity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(actual, linkEntity);
        AssertThatRecordsMatch(actual.Tag.Value, SeededTagRecords[linkEntity.TagId]);
        AssertThatRecordsMatch(actual.Entity.Value, SeededMasterRecords[linkEntity.EntityId]);
    }
    
    [Test]
    public void Upsert_WhenNewLinkedTag_ItIsInsertedAccurately()
    {
        var linkEntity = new TestEntityTagLink { EntityId = 5 };
        var usedTagIds = SeededLinkRecords.Where(x => x.Value.EntityId == linkEntity.EntityId)
            .Select(x => x.Value.TagId).ToArray();
        var availableTagId = SeededTagRecords.Keys.First(x => !usedTagIds.Contains(x));
        linkEntity.TagId = availableTagId;
        
        var ret = Orm.Upsert(linkEntity);
        Assert.That(ret, Is.EqualTo(UpsertResult.Inserted));
        
        var actual = Orm
            .Get<TestEntityTagLink>(recursiveLoad: true)
            .Where(x => x.Id == linkEntity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(actual, linkEntity);
        AssertThatRecordsMatch(actual.Tag.Value, SeededTagRecords[linkEntity.TagId]);
        AssertThatRecordsMatch(actual.Entity.Value, SeededMasterRecords[linkEntity.EntityId]);
    }
    
    [Test]
    public void UpsertMany_WhenAllNewRecords_AreInsertedAccurately()
    {
        var entity1 = CreateTestEntityMasterWithRandomValues();
        var entity2 = CreateTestEntityMasterWithRandomValues();
        var entity3 = CreateTestEntityMasterWithRandomValues();
        TestEntityMaster[] entities = [entity1, entity2, entity3];

        var ret = Orm.UpsertMany(entities);
        
        Assert.That(ret.InsertCount, Is.EqualTo(3));
        Assert.That(ret.UpdateCount, Is.EqualTo(0));
        Assert.That(ret.FailedCount, Is.EqualTo(0));
        Assert.That(entity1.Id, Is.EqualTo(SeededMasterRecords.Count + 1));
        Assert.That(entity2.Id, Is.EqualTo(SeededMasterRecords.Count + 2));
        Assert.That(entity3.Id, Is.EqualTo(SeededMasterRecords.Count + 3));

        var insertedIds = entities.Select(y => y.Id).ToArray();
        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => insertedIds.Contains(x.Id))
            .AsEnumerable()
            .ToArray();
        
        Assert.That(actual.Length, Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++)
            AssertThatRecordsMatch(actual[i], entities[i]);
    } 
    
    [Test]
    public void UpsertMany_WhenAllUpdatedRecords_AreUpdatedAccurately()
    {
        var entity1 = CreateTestEntityMasterWithRandomValues();
        entity1.Id = 2;
        var entity2 = CreateTestEntityMasterWithRandomValues();
        entity2.Id = 3;
        var entity3 = CreateTestEntityMasterWithRandomValues();
        entity3.Id = 4;
        TestEntityMaster[] entities = [entity1, entity2, entity3];

        var ret = Orm.UpsertMany(entities);
        
        Assert.That(ret.InsertCount, Is.EqualTo(0));
        Assert.That(ret.UpdateCount, Is.EqualTo(3));
        Assert.That(ret.FailedCount, Is.EqualTo(0));

        var insertedIds = entities.Select(y => y.Id).ToArray();
        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => insertedIds.Contains(x.Id))
            .AsEnumerable()
            .ToArray();
        
        Assert.That(actual.Length, Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++)
            AssertThatRecordsMatch(actual[i], entities[i]);
    }
    
    [Test]
    public void UpsertMany_WhenMixOfUpdatedAndNewRecords_AreStoredAccurately()
    {
        var entity1 = CreateTestEntityMasterWithRandomValues();
        var entity2 = CreateTestEntityMasterWithRandomValues();
        entity2.Id = 2;
        var entity3 = CreateTestEntityMasterWithRandomValues();
        var entity4 = CreateTestEntityMasterWithRandomValues();
        entity4.Id = 4;
        TestEntityMaster[] entities = [entity1, entity2, entity3, entity4];

        var ret = Orm.UpsertMany(entities);
        
        entities = entities.OrderBy(x => x.Id).ToArray();
        Assert.That(ret.InsertCount, Is.EqualTo(2));
        Assert.That(ret.UpdateCount, Is.EqualTo(2));
        Assert.That(ret.FailedCount, Is.EqualTo(0));
        Assert.That(entity1.Id, Is.EqualTo(SeededMasterRecords.Count + 1));
        Assert.That(entity3.Id, Is.EqualTo(SeededMasterRecords.Count + 2));

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => entities.Select(y => y.Id).Contains(x.Id))
            .OrderBy(x => x.Id)
            .AllRecords();
        
        Assert.That(actual.Length, Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++)
            AssertThatRecordsMatch(actual[i], entities[i]);
    }  
}