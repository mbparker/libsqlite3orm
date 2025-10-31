using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

[TestFixture]
public class ODataQueryHandlerTests : IntegrationTestSeededBase<TestDbContext>
{
    [Test]
    public void ODataQuery_WhenFilterBySingleId_ReturnsExpectedResult()
    {
        var expected = SeededMasterRecords[1];

        var actual = Orm.ODataQuery<TestEntityMaster>("$filter=id eq 1");
        
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(1));
        AssertThatRecordsMatch(actualEntities[0],  expected);
    }
    
    [Test]
    public void ODataQuery_WhenFilterByLinkedValue_ReturnsExpectedResult()
    {
        var firstTagValue = SeededTagRecords.Values.First().TagValue;
        var expected = SeededLinkRecords.Values.Where(x => x.Tag.Value.TagValue == firstTagValue).ToArray();

        var actual = Orm.ODataQuery<TestEntityTagLink>($"$filter=tag.value.tagValue eq '{firstTagValue}'");
        
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }    
    
    [Test]
    public void ODataQuery_WhenFilterByNegatedId_ReturnsExpectedResults()
    {
        var expected = SeededMasterRecords.Values.ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$filter=id gt -1");
        
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }    
    
    [Test]
    public void ODataQuery_WhenHasOrderBy_ReturnsExpectedResults()
    {
        var expected = SeededMasterRecords.Values.OrderByDescending(x => x.StringValue).ThenBy(x => x.DoubleValue).ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$orderby=stringValue desc,doubleValue asc");
        
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }
    
    [Test]
    public void ODataQuery_WhenHasComplexOrderBy_ReturnsExpectedResults()
    {
        var expected = SeededLinkRecords.Values.OrderByDescending(x => x.Tag.Value?.TagValue).ToArray();

        var actual = Orm.ODataQuery<TestEntityTagLink>("$orderby=tag.value.tagValue desc");
        
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }    
    
    [Test]
    public void ODataQuery_WhenHasOrderByAndPaging_ReturnsExpectedResults()
    {
        var expected = SeededMasterRecords.Values.OrderByDescending(x => x.StringValue).ThenBy(x => x.DoubleValue).Skip(2).Take(5).ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$orderby=stringValue desc,doubleValue asc&$top=5&$skip=2");
        
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }    
    
    [Test]
    public void ODataQuery_WhenHasOrderByAndPagingWithCount_ReturnsExpectedResults()
    {
        var expected = SeededMasterRecords.Values.OrderByDescending(x => x.StringValue).ThenBy(x => x.DoubleValue).Skip(2).Take(5).ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$orderby=stringValue desc,doubleValue asc&$top=5&$skip=2&$count=true");
        
        Assert.That(actual.Count, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(SeededMasterRecords.Count));
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }     
    
    [Test]
    public void ODataQuery_WhenHasFilteringAndOrderByAndPagingWithCount_ReturnsExpectedResults()
    {
        var expected = SeededMasterRecords.Values.Where(x => x.EnumValue == TestEntityKind.Kind1 || x.EnumValue == TestEntityKind.Kind2)
            .OrderByDescending(x => x.StringValue).ThenBy(x => x.DoubleValue).Skip(2).Take(5).ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$filter=enumValue eq 0 or enumValue eq 1&$orderby=stringValue desc,doubleValue asc&$top=5&$skip=2&$count=true");
        
        Assert.That(actual.Count, Is.Not.Null);
        Assert.That(actual.Count,
            Is.EqualTo(SeededMasterRecords.Count(x => x.Value.EnumValue == TestEntityKind.Kind1 || x.Value.EnumValue == TestEntityKind.Kind2)));
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities, Is.Not.Empty);
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }
    
    [Test]
    public void ODataQuery_WhenHasComplexFilteringAndOrderByAndPagingWithCount_ReturnsExpectedResults()
    {
        var expected = SeededMasterRecords.Values.Where(x => (x.EnumValue == TestEntityKind.Kind2 && x.DoubleValue < 50125175.126) || !(x.ByteValue > 127))
            .OrderByDescending(x => x.StringValue).ThenBy(x => x.DoubleValue).Skip(2).Take(5).ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$filter=((enumValue eq 1) and (doubleValue lt 50125175.126)) or (not(byteValue gt 127))&$orderby=stringValue desc,doubleValue asc&$top=5&$skip=2&$count=true");
        
        Assert.That(actual.Count, Is.Not.Null);
        Assert.That(actual.Count,
            Is.EqualTo(SeededMasterRecords.Count(x => (x.Value.EnumValue == TestEntityKind.Kind2 && x.Value.DoubleValue < 50125175.126) || !(x.Value.ByteValue > 127))));
        var actualEntities = actual.Entities.ToArray();
        Assert.That(actualEntities.Length, Is.EqualTo(expected.Length));
        for (var i = 0; i < expected.Length; i++)
            AssertThatRecordsMatch(actualEntities[i], expected[i]);
    }      
    
    [Test]
    public void ODataQuery_WhenCountOnly_IncludesCount()
    {
        var expected = SeededMasterRecords.Values.ToArray();

        var actual = Orm.ODataQuery<TestEntityMaster>("$count=true");
        
        Assert.That(actual.Count, Is.Not.Null);
        Assert.That(actual.Count, Is.EqualTo(expected.Length));
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    }

    [Test]
    public void ODataQuery_WhenContainsCall_ReturnsExpectedResults()
    {
        var firstWords = SeededMasterRecords.Values.First(x => x.StringValue.Split(' ').Length > 1).StringValue
            .Split(' ');
        var expected = SeededMasterRecords.Values.Where(x => x.StringValue.Contains(firstWords[0])).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityMaster>($"$filter=contains(stringValue, '{firstWords[0]}')");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    }
    
    [Test]
    public void ODataQuery_WhenStartsWithCall_ReturnsExpectedResults()
    {
        var firstWords = SeededMasterRecords.Values.First(x => x.StringValue.Split(' ').Length > 1).StringValue
            .Split(' ');        
        var expected = SeededMasterRecords.Values.Where(x => x.StringValue.StartsWith(firstWords[0])).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityMaster>($"$filter=startswith(stringValue, '{firstWords[0]}')");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    } 
    
    [Test]
    public void ODataQuery_WhenStartsWithCallOnNestedProperty_ReturnsExpectedResults()
    {
        var firstWords = SeededTagRecords.Values.First(x => x.TagValue.Split(' ').Length > 1).TagValue
            .Split(' ');        
        var expected = SeededLinkRecords.Values.Where(x => x.Tag.Value.TagValue.StartsWith(firstWords[0])).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityTagLink>($"$filter=startswith(tag.value.tagValue, '{firstWords[0]}')");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    }     
    
    [Test]
    public void ODataQuery_WhenEndsWithCall_ReturnsExpectedResults()
    {
        var firstWords = SeededMasterRecords.Values.First(x => x.StringValue.Split(' ').Length > 1).StringValue
            .Split(' ');         
        var expected = SeededMasterRecords.Values.Where(x => x.StringValue.EndsWith(firstWords[^1])).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityMaster>($"$filter=endswith(stringValue, '{firstWords[^1]}')");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    }
    
    [Test]
    public void ODataQuery_WhenToLowerCall_ReturnsExpectedResults()
    {
        var firstValue = SeededMasterRecords.Values.First(x => x.StringValue.Split(' ').Length > 1).StringValue.ToLower();
        var expected = SeededMasterRecords.Values.Where(x => x.StringValue.ToLower() == firstValue).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityMaster>($"$filter=toLower(stringValue) eq '{firstValue}'");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    }      
    
    [Test]
    public void ODataQuery_WhenToUpperCall_ReturnsExpectedResults()
    {
        var firstValue = SeededMasterRecords.Values.First(x => x.StringValue.Split(' ').Length > 1).StringValue.ToUpper();
        var expected = SeededMasterRecords.Values.Where(x => x.StringValue.ToUpper() == firstValue).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityMaster>($"$filter=toUpper(stringValue) eq '{firstValue}'");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    } 
    
    [Test]
    public void ODataQuery_WhenToUpperAndContainsCalls_ReturnsExpectedResults()
    {
        var firstValue = SeededMasterRecords.Values.First(x => x.StringValue.Split(' ').Length > 1).StringValue;
        var expected = SeededMasterRecords.Values.Where(x => x.StringValue.ToUpper().Contains(firstValue.ToUpper())).ToArray();
        
        var actual = Orm.ODataQuery<TestEntityMaster>($"$filter=contains(toUpper(stringValue), toUpper('{firstValue}'))");
        
        Assert.That(actual.Entities, Is.Not.Empty);
        Assert.That(actual.Entities.Count(), Is.EqualTo(expected.Length));
    }    
}