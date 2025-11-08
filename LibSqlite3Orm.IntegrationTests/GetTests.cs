using System.Linq.Expressions;
using LibSqlite3Orm.IntegrationTests.TestDataModel;
using LibSqlite3Orm.Models.Orm.Events;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class GetTests : IntegrationTestSeededBase<TestDbContext>
{
    private List<CacheAccessAttemptEventArgs> cacheGetEvents = new();
    
    [Test]
    public void Get_WhenRepeatedReadsOfSameNavProp_UsesCachedValue()
    {
        var missCount = 0;
        var optionalRecordIds = new HashSet<long>();
        for (var i = 1; i <= 10; i++)
        {
            var records = Orm
                .Get<TestEntityMaster>(true)
                .OrderBy(x => x.Id)
                .AsEnumerable();
            
            foreach (var record in records)
            {
                var details = record.OptionalDetail.Value;
                if (details is not null)
                {
                    optionalRecordIds.Add(details.Id);
                    Assert.That(cacheGetEvents.Count, Is.EqualTo(1));
                    var lastEvent = cacheGetEvents.First();
                    if (lastEvent.IsHit && lastEvent.DetailEntity is Lazy<TestEntityOptionalDetail> detailEntity)
                    {
                        AssertThatRecordsMatch(details, detailEntity.Value);
                    }
                    else
                    {
                        if (lastEvent.IsHit)
                            Assert.Fail($"Detail entity is wrong type: {lastEvent.DetailEntity.GetType().Name}");
                        else
                            missCount++;
                    }
                }
                else
                {
                    Assert.That(cacheGetEvents.Count, Is.EqualTo(0));
                }
                
                cacheGetEvents.Clear();
            }
        }

        Assert.That(missCount, Is.EqualTo(optionalRecordIds.Count));
    }

    [Test]
    public void Get_WhenDetailEntityIsModified_RemovesCacheEntry()
    {
        var records = Orm
            .Get<TestEntityMaster>(true)
            .Where(x => x.OptionalDetailId != null)
            .OrderBy(x => x.Id)
            .AllRecords();

        var detail = records[0].OptionalDetail.Value;
        detail.Details = "Some modified text";
        Orm.Update(detail);
        cacheGetEvents.Clear();

        var rec = Orm
            .Get<TestEntityMaster>(true)
            .Where(x => x.OptionalDetailId != null)
            .OrderBy(x => x.Id)
            .Take(1)
            .SingleRecord();
        
        detail = rec.OptionalDetail.Value;
        Assert.That(cacheGetEvents.Count, Is.EqualTo(1));
        var lastEvent = cacheGetEvents.First();
        Assert.That(lastEvent.IsHit, Is.False);
    }
    
    [Test]
    public void Get_WhenDetailEntityIsDeleted_RemovesCacheEntry()
    {
        var rec = Orm
            .Get<TestEntityMaster>(true)
            .Where(x => x.OptionalDetailId != null)
            .OrderBy(x => x.Id)
            .Take(1)
            .SingleRecord();

        var detail = rec.OptionalDetail.Value;
        rec.OptionalDetailId = null;
        Assert.That(() => Orm.Update(rec), Is.True);
        Assert.That(() => Orm.Delete<TestEntityOptionalDetail>(x => x.Id == detail.Id), Is.EqualTo(1));
        cacheGetEvents.Clear();

        rec = Orm
            .Get<TestEntityMaster>(true)
            .Where(x => x.OptionalDetailId != null)
            .OrderBy(x => x.Id)
            .Take(1)
            .SingleRecord();
        
        var detail2 = rec.OptionalDetail.Value;
        Assert.That(cacheGetEvents.Count, Is.EqualTo(1));
        var lastEvent = cacheGetEvents.First();
        Assert.That(lastEvent.IsHit, Is.False);
        Assert.That(detail2.Id, Is.Not.EqualTo(detail.Id));
    }

    [Test]
    public void Get_WhenCacheDisabled_ProducesNoCacheEventsAndReturnsExpectedData()
    {
        Orm.DisableCaching = true;
        
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
        
        Assert.That(cacheGetEvents, Is.Empty);
    }

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
    
    public override void SetUp()
    {
        base.SetUp();
        LogicTracer.CachedGetAttempt += HandleCachedGetAttempt;
    }

    public override void TearDown()
    {
        LogicTracer.CachedGetAttempt -= HandleCachedGetAttempt;
        cacheGetEvents.Clear();
        base.TearDown();
    }
    
    private void HandleCachedGetAttempt(object sender, CacheAccessAttemptEventArgs e)
    {
        cacheGetEvents.Add(e);
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