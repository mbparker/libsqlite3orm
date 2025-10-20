using LibSqlite3Orm.IntegrationTests.TestDataModel;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class UpdateTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void Update_WhenMasterValuesChange_TheyAreStoredAccurately()
    {
        var entity = CreateTestEntityMasterWithRandomValues();
        entity.Id = SeededMasterRecords[1].Id;
        var ret = Orm.Update(entity);

        Assert.That(ret, Is.True);

        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }
    
    [Test]
    public void Update_WhenTagValuesChange_TheyAreStoredAccurately()
    {
        var entity = CreateTestEntityTagWithRandomValues();
        entity.Id = SeededTagRecords[1].Id;
        var ret = Orm.Update(entity);

        Assert.That(ret, Is.True);
        
        var actual = Orm
            .Get<TestEntityTag>()
            .Where(x => x.Id == entity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(entity, actual);
    }

    [Test]
    public void Update_WhenLinkedTagIdChanges_ItIsStoredAccurately()
    {
        var linkEntity = SeededLinkRecords.Values.First();
        var usedTagIds = SeededLinkRecords.Where(x => x.Value.EntityId == linkEntity.EntityId)
            .Select(x => x.Value.TagId).ToArray();
        var availableTagId = SeededTagRecords.Keys.First(x => !usedTagIds.Contains(x));
        
        linkEntity.TagId = availableTagId;
        var ret = Orm.Update(linkEntity);

        Assert.That(ret, Is.True);
        
        var actual = Orm
            .Get<TestEntityTagLink>(recursiveLoad: true)
            .Where(x => x.Id == linkEntity.Id)
            .SingleRecord();
        
        AssertThatRecordsMatch(linkEntity, actual);
        AssertThatRecordsMatch(SeededTagRecords[linkEntity.TagId], actual.Tag.Value);
        AssertThatRecordsMatch(SeededMasterRecords[linkEntity.EntityId], actual.Entity.Value);
    }
    
    [Test]
    public void Update_WhenLinkedTagIdChangesButViolatesConstraint_Throws()
    {
        var grouping = SeededLinkRecords.Values.GroupBy(x => x.EntityId).First(x => x.Count() > 1);
        var linksForEntity = grouping.ToArray();
        var linkEntity = linksForEntity[0];
        
        linkEntity.TagId = linksForEntity[1].TagId;
        
        var ex = Assert.Throws<SqliteException>(() => Orm.Update(linkEntity));
        Assert.That(ex?.Message, Is.EqualTo("UNIQUE constraint failed: TestEntityTagLink.TagId, TestEntityTagLink.EntityId"));
    }
    
    [Test]
    public void Update_WhenTagValueViolatesConstraint_Throws()
    {
        var entity = SeededTagRecords[1];
        
        entity.TagValue = SeededTagRecords[2].TagValue;
        
        var ex = Assert.Throws<SqliteException>(() => Orm.Update(entity));
        Assert.That(ex?.Message, Is.EqualTo("UNIQUE constraint failed: TestEntityTag.TagValue"));
    }
    
    [Test]
    public void Update_WhenTagDoesNotExist_ReturnsFalse()
    {
        var entity = SeededTagRecords[1];
        
        entity.TagValue = SeededTagRecords[2].TagValue;
        entity.Id = 0;

        var ret = Orm.Update(entity);
        
        Assert.That(ret, Is.False);
    }
    
    [Test]
    public void UpdateMany_WhenAllUpdatedRecords_AreUpdatedAccurately()
    {
        var entity1 = CreateTestEntityMasterWithRandomValues();
        entity1.Id = 2;
        var entity2 = CreateTestEntityMasterWithRandomValues();
        entity2.Id = 3;
        var entity3 = CreateTestEntityMasterWithRandomValues();
        entity3.Id = 4;
        TestEntityMaster[] entities = [entity1, entity2, entity3];

        var ret = Orm.UpdateMany(entities);
        
        Assert.That(ret, Is.EqualTo(3));
        var insertedIds = entities.Select(y => y.Id).ToArray();
        var actual = Orm
            .Get<TestEntityMaster>()
            .Where(x => insertedIds.Contains(x.Id))
            .AsEnumerable()
            .ToArray();
        
        Assert.That(actual.Length, Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++)
            AssertThatRecordsMatch(entities[i], actual[i]);
    }
}