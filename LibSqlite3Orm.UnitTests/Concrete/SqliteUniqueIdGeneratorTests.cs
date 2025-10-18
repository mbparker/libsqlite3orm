using LibSqlite3Orm.Concrete;

namespace LibSqlite3Orm.UnitTests.Concrete;

[TestFixture]
public class SqliteUniqueIdGeneratorTests
{
    private SqliteUniqueIdGenerator _generator;

    [SetUp]
    public void SetUp()
    {
        _generator = new SqliteUniqueIdGenerator();
    }

    [Test]
    public void NewUniqueId_GeneratesValidGuidString()
    {
        // Act
        var result = _generator.NewUniqueId();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
        Assert.That(result.Length, Is.EqualTo(32), "GUID without hyphens should be 32 characters");
        Assert.That(result, Does.Match("^[a-f0-9]{32}$"), "Should contain only lowercase hex characters");
    }

    [Test]
    public void NewUniqueId_GeneratesUniqueValues()
    {
        // Act
        var id1 = _generator.NewUniqueId();
        var id2 = _generator.NewUniqueId();
        var id3 = _generator.NewUniqueId();

        // Assert
        Assert.That(id1, Is.Not.EqualTo(id2));
        Assert.That(id2, Is.Not.EqualTo(id3));
        Assert.That(id1, Is.Not.EqualTo(id3));
    }

    [Test]
    public void NewUniqueId_ParsedAsGuid_IsValid()
    {
        // Act
        var result = _generator.NewUniqueId();

        // Assert
        Assert.DoesNotThrow(() => new Guid(result), "Generated ID should be parseable as GUID");
    }

    [Test]
    public void NewUniqueId_CalledMultipleTimes_AlwaysReturnsValidFormat()
    {
        // Act & Assert
        for (int i = 0; i < 100; i++)
        {
            var id = _generator.NewUniqueId();
            Assert.That(id, Is.Not.Null);
            Assert.That(id.Length, Is.EqualTo(32));
            Assert.That(id, Does.Match("^[a-f0-9]{32}$"));
        }
    }
}