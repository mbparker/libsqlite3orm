using System.Globalization;
using System.Linq.Expressions;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class GetWithCustomCollationTests : IntegrationTestSeededBase<TestDbContextWithCustomCollation>
{
    private int collateFuncInvocations;
    
    [TestCaseSource(nameof(StringValuesTestCaseSource))]
    public void Get_WhenFilterOnStringEndsWithLowerButCompareValueIsUpper_ReturnsEmptyRecordSet(bool recursiveLoad, string value)
    {
        var count = 0;
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => x.StringValue.ToLower().EndsWith(value.ToUpper()), x => x.Id, (actual, expected) =>
            {
                count++;
                if (recursiveLoad)
                {
                    AssertThatTagLinkRecordsMatch(actual, expected);
                    AssertOptionalRecordsMatch(actual, expected);
                }
                else
                {
                    Assert.That(actual.Tags.Value, Is.Null);
                    Assert.That(actual.OptionalDetail.Value, Is.Null);
                }
            });    
        
        Assert.That(count, Is.EqualTo(0));
        Assert.That(collateFuncInvocations, Is.GreaterThan(0));
    }   

    protected override void RegisterCustomCollations(ISqliteCustomCollationRegistry registry)
    {
        registry.RegisterCustomCollation("TEST_COLLATION", CollateFunc);
    }

    private int CollateFunc(string s1, string s2)
    {
        collateFuncInvocations++;
        return string.Compare(s1, s2, StringComparison.CurrentCultureIgnoreCase);
    }
    
    private void Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder<TEntity, TKey>(bool recursiveLoad,
        Dictionary<long, TEntity> expectedDataSource, Expression<Func<TEntity, bool>> filterExpression,
        Expression<Func<TEntity, TKey>> sortExpression, Action<TEntity, TEntity> assertNavPropsAction = null) where TEntity : new()
    {
        var actual = Orm
            .Get<TEntity>(recursiveLoad)
            .Where(filterExpression)
            .OrderBy(sortExpression)
            .AllRecords();

        var expected = expectedDataSource
            .Values
            .Where(filterExpression.Compile())
            .OrderBy(sortExpression.Compile())
            .ToArray();

        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
        {
            AssertThatRecordsMatch(actual[i], expected[i]);
            assertNavPropsAction?.Invoke(actual[i], expected[i]);
        }
    }

    private void AssertThatTagLinkRecordsMatch(TestEntityMaster actual, TestEntityMaster expected)
    {
        Assert.That(actual.Tags.Value, Is.Not.Null);
            
        var expectedTagLinks = SeededLinkRecords
            .Values
            .Where(x => x.EntityId == expected.Id)
            .OrderBy(x => x.TagId)
            .ToArray();            
            
        var actualTagLinks = actual.Tags.Value
            .AllRecords()
            .OrderBy(x => x.TagId)
            .ToArray();
            
        Assert.That(actualTagLinks.Length, Is.EqualTo(expectedTagLinks.Length));
        for (var j = 0; j < expectedTagLinks.Length; j++)
        {
            var expectedTagLink = expectedTagLinks[j];
            var actualTagLink =  actualTagLinks[j];
            AssertThatRecordsMatch(actualTagLink, expectedTagLink);
            Assert.That(actualTagLink.Tag.Value, Is.Not.Null);
            AssertThatRecordsMatch(actualTagLink.Tag.Value, SeededTagRecords[expectedTagLink.TagId]);
            Assert.That(actualTagLink.Entity.Value, Is.Not.Null);
            AssertThatRecordsMatch(actualTagLink.Entity.Value, SeededMasterRecords[expectedTagLink.EntityId]);
        }        
    }

    private void AssertOptionalRecordsMatch(TestEntityMaster actual, TestEntityMaster expected)
    {
        Assert.That(actual.OptionalDetailId, Is.EqualTo(expected.OptionalDetailId));
        if (expected.OptionalDetailId.HasValue)
        {
            var expectedOptionalRecord = SeededOptionalRecords[expected.OptionalDetailId.Value];
            Assert.That(actual.OptionalDetail.Value, Is.Not.Null);
            Assert.That(actual.OptionalDetail.Value.Id, Is.EqualTo(expectedOptionalRecord.Id));
            Assert.That(actual.OptionalDetail.Value.Details, Is.EqualTo(expectedOptionalRecord.Details));
            Assert.That(actual.OptionalDetail.Value.Date, Is.EqualTo(expectedOptionalRecord.Date));
        }
        else
        {
            Assert.That(actual.OptionalDetail.Value, Is.Null);
        }
    }    
    
    private static IEnumerable<object[]> StringValuesTestCaseSource()
    {
        foreach (var word in WordList)
        {
            yield return [false, word];
            yield return [true, word];
        }
    }    
}