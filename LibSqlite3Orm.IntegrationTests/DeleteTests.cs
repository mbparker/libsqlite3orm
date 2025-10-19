using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class DeleteTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void DeleteAll_WhenInvokedOnMaster_RemovesAllMasterAndLinkRecords()
    {
        Orm.DeleteAll<TestEntityMaster>();

        var actualMaster = Orm.Get<TestEntityMaster>().Count();
        var actualLink = Orm.Get<TestEntityTagLink>().Count();
        var actualTag = Orm.Get<TestEntityTag>().Count();

        Assert.That(actualMaster, Is.EqualTo(0));
        Assert.That(actualLink, Is.EqualTo(0));
        Assert.That(actualTag, Is.EqualTo(SeededTagRecords.Count));
    }
    
    [Test]
    public void Delete_WhenInvokedOnMaster_RemovesCorrectMasterAndLinkRecords()
    {
        var grouping = SeededLinkRecords.Values.GroupBy(x => x.EntityId).First(x => x.Count() > 1);
        var linksForEntity = grouping.ToArray();
        var masterIdToDelete = linksForEntity[0].EntityId;
        var startingLinkTotalCount = Orm.Get<TestEntityTagLink>().Count();
        var expectedLinkDeletionCount = Orm.Get<TestEntityTagLink>().Count(x => x.EntityId == masterIdToDelete);
        
        Assert.That(startingLinkTotalCount, Is.Not.EqualTo(0));
        Assert.That(expectedLinkDeletionCount, Is.Not.EqualTo(0));
        
        Orm.Delete<TestEntityMaster>(x => x.Id == masterIdToDelete);

        var actualMasterCount = Orm.Get<TestEntityMaster>().Count();
        var actualLinkCount = Orm.Get<TestEntityTagLink>().Count();

        Assert.That(actualMasterCount, Is.EqualTo(SeededMasterRecords.Count - 1));
        Assert.That(actualLinkCount, Is.EqualTo(startingLinkTotalCount - expectedLinkDeletionCount));

        var actualDeletedMasterRecord = Orm
            .Get<TestEntityMaster>()
            .Where(x => x.Id == masterIdToDelete)
            .SingleRecord();
        var actualDeletedLinkRecords = Orm
            .Get<TestEntityTagLink>()
            .Where(x => x.EntityId == masterIdToDelete)
            .AllRecords();
        
        Assert.That(actualDeletedMasterRecord, Is.Null);
        Assert.That(actualDeletedLinkRecords, Is.Empty);
    }    
    
    [Test]
    public void DeleteAll_WhenInvokedOnTag_RemovesAllTagAndLinkRecords()
    {
        Orm.DeleteAll<TestEntityTag>();

        var actualMaster = Orm.Get<TestEntityMaster>().Count();
        var actualLink = Orm.Get<TestEntityTagLink>().Count();
        var actualTag = Orm.Get<TestEntityTag>().Count();

        Assert.That(actualMaster, Is.EqualTo(SeededMasterRecords.Count));
        Assert.That(actualLink, Is.EqualTo(0));
        Assert.That(actualTag, Is.EqualTo(0));
    }
    
    [Test]
    public void Delete_WhenInvokedOnTag_RemovesCorrectTagAndLinkRecords()
    {
        var grouping = SeededLinkRecords.Values.GroupBy(x => x.TagId).First(x => x.Count() > 1);
        var linksForTag = grouping.ToArray();
        var tagIdToDelete = linksForTag[0].TagId;
        var startingLinkTotalCount = Orm.Get<TestEntityTagLink>().Count();
        var expectedLinkDeletionCount = Orm.Get<TestEntityTagLink>().Count(x => x.TagId == tagIdToDelete);
        
        Assert.That(startingLinkTotalCount, Is.Not.EqualTo(0));
        Assert.That(expectedLinkDeletionCount, Is.Not.EqualTo(0));
        
        Orm.Delete<TestEntityTag>(x => x.Id == tagIdToDelete);

        var actualTagCount = Orm.Get<TestEntityTag>().Count();
        var actualLinkCount = Orm.Get<TestEntityTagLink>().Count();

        Assert.That(actualTagCount, Is.EqualTo(SeededTagRecords.Count - 1));
        Assert.That(actualLinkCount, Is.EqualTo(startingLinkTotalCount - expectedLinkDeletionCount));

        var actualDeletedTagRecord = Orm
            .Get<TestEntityTag>()
            .Where(x => x.Id == tagIdToDelete)
            .SingleRecord();
        var actualDeletedLinkRecords = Orm
            .Get<TestEntityTagLink>()
            .Where(x => x.TagId == tagIdToDelete)
            .AllRecords();
        
        Assert.That(actualDeletedTagRecord, Is.Null);
        Assert.That(actualDeletedLinkRecords, Is.Empty);
    }        
    
    [Test]
    public void DeleteAll_WhenInvokedOnLink_RemovesAllLinkRecords()
    {
        Orm.DeleteAll<TestEntityTagLink>();

        var actualMaster = Orm.Get<TestEntityMaster>().Count();
        var actualLink = Orm.Get<TestEntityTagLink>().Count();
        var actualTag = Orm.Get<TestEntityTag>().Count();

        Assert.That(actualMaster, Is.EqualTo(SeededMasterRecords.Count));
        Assert.That(actualLink, Is.EqualTo(0));
        Assert.That(actualTag, Is.EqualTo(SeededTagRecords.Count));
    }      
}