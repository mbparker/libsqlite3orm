using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class GetTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void Get_WhenNoWhereClauseAndNotRecursive_ReturnsAllRecordsWithoutNavigationProps()
    {
        var actual = Orm
            .Get<TestEntityMaster>()
            .OrderBy(x => x.Id)
            .AllRecords();

        var expected = SeededMasterRecords.Values.OrderBy(x => x.Id).ToArray();
        Assert.That(actual.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++)
        {
            AssertThatRecordsMatch(expected[i], actual[i]);
            Assert.That(actual[i].Tags.Value, Is.Null);
        }
    }
    
    [Test]
    public void Get_WhenNoWhereClauseAndRecursive_ReturnsAllRecordsWithNavigationProps()
    {
        var expected = SeededMasterRecords.Values.OrderBy(x => x.Id).ToArray();
        
        var actual = Orm
            .Get<TestEntityMaster>(recursiveLoad: true)
            .OrderBy(x => x.Id)
            .AllRecords();
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length));

        for (var i = 0; i < expected.Length; i++)
        {
            AssertThatRecordsMatch(expected[i], actual[i]);
            Assert.That(actual[i].Tags.Value, Is.Not.Null);
            var expectedTagLinks = SeededLinkRecords.Values.Where(x => x.EntityId == expected[i].Id)
                .OrderBy(x => x.TagId).ToArray();            
            var actualTagLinks = actual[i].Tags.Value.AllRecords().OrderBy(x => x.TagId).ToArray();
            Assert.That(actualTagLinks.Length, Is.EqualTo(expectedTagLinks.Length));
            for (var j = 0; j < expectedTagLinks.Length; j++)
            {
                var expectedTagLink = expectedTagLinks[j];
                var actualTagLink =  actualTagLinks[j];
                AssertThatRecordsMatch(expectedTagLink, actualTagLink);
                Assert.That(actualTagLink.Tag.Value, Is.Not.Null);
                AssertThatRecordsMatch(SeededTagRecords[expectedTagLink.TagId], actualTagLink.Tag.Value);
                Assert.That(actualTagLink.Entity.Value, Is.Not.Null);
                AssertThatRecordsMatch(SeededMasterRecords[expectedTagLink.EntityId], actualTagLink.Entity.Value);
            }
        }
    }    
}