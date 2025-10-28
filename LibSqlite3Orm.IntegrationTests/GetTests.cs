using System.Linq.Expressions;
using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class GetTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void Get_WhenNoWhereClauseAndNotRecursive_ReturnsAllRecordsWithoutNavigationProps()
    {
        var expected = SeededMasterRecords
            .Values
            .OrderBy(x => x.Id)
            .ToArray();
        
        var actual = Orm
            .Get<TestEntityMaster>()
            .OrderBy(x => x.Id)
            .AllRecords();
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
        {
            AssertThatRecordsMatch(actual[i], expected[i]);
            Assert.That(actual[i].Tags.Value, Is.Null);
        }
    }
    
    [Test]
    public void Get_WhenNoWhereClauseAndRecursive_ReturnsAllRecordsWithNavigationProps()
    {
        var expected = SeededMasterRecords
            .Values
            .OrderBy(x => x.Id)
            .ToArray();
        
        var actual = Orm
            .Get<TestEntityMaster>(recursiveLoad: true)
            .OrderBy(x => x.Id)
            .AllRecords();
        
        Assert.That(actual.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
        {
            AssertThatRecordsMatch(actual[i], expected[i]);
            AssertThatTagLinkRecordsMatch(actual[i], expected[i]);
            AssertOptionalRecordsMatch(actual[i], expected[i]);
        }
    }
    
    [TestCase(false, TestEntityKind.Kind1)]
    [TestCase(false, TestEntityKind.Kind2)]
    [TestCase(true, TestEntityKind.Kind1)]
    [TestCase(true, TestEntityKind.Kind2)]    
    public void Get_WhenFilterOnEnum_ReturnsCorrectRecordSet(bool recursiveLoad, TestEntityKind enumVal)
    {
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => x.EnumValue == enumVal, x => x.Id, (actual, expected) =>
            {
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
    }
    
    [TestCaseSource(nameof(StringValuesTestCaseSource))]
    public void Get_WhenFilterOnStringContains_ReturnsCorrectRecordSet(bool recursiveLoad, string value)
    {
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => x.StringValue.Contains(value), x => x.Id, (actual, expected) =>
            {
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
    }
    
    [TestCaseSource(nameof(StringValuesTestCaseSource))]
    public void Get_WhenFilterOnStringDoesNotContain_ReturnsCorrectRecordSet(bool recursiveLoad, string value)
    {
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => !x.StringValue.Contains(value), x => x.Id, (actual, expected) =>
            {
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
    }
    
    [TestCaseSource(nameof(StringValuesTestCaseSource))]
    public void Get_WhenFilterOnStringStartsWith_ReturnsCorrectRecordSet(bool recursiveLoad, string value)
    {
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => x.StringValue.StartsWith(value), x => x.Id, (actual, expected) =>
            {
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
    }
    
    [TestCaseSource(nameof(StringValuesTestCaseSource))]
    public void Get_WhenFilterOnStringEndsWith_ReturnsCorrectRecordSet(bool recursiveLoad, string value)
    {
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => x.StringValue.EndsWith(value), x => x.Id, (actual, expected) =>
            {
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
    }
    
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
    }    
    
    [TestCase(false, 1, 10)]
    [TestCase(false, 11, 30)]
    [TestCase(true, 1, 10)]
    [TestCase(true, 11, 30)]    
    public void Get_WhenFilterOnId_ReturnsCorrectRecordSet(bool recursiveLoad, long idLow, long idHigh)
    {
        Get_WhenFilterAndSortExpressions_ReturnsExpectedRecordsInCorrectOrder(recursiveLoad, SeededMasterRecords,
            x => x.Id >= idLow && x.Id <= idHigh, x => x.Id, (actual, expected) =>
            {
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