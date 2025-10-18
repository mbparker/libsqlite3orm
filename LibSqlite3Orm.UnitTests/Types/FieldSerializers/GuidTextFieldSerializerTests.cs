using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class GuidTextFieldSerializerTests
{
    private GuidTextFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new GuidTextFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsGuidType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(Guid)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithValidGuid_ReturnsFormattedString()
    {
        // Arrange
        var guid = new Guid("12345678-1234-1234-1234-123456789abc");

        // Act
        var result = _serializer.Serialize(guid);

        // Assert
        Assert.That(result, Is.EqualTo("12345678-1234-1234-1234-123456789abc"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Serialize_WithEmptyGuid_ReturnsEmptyGuidString()
    {
        // Arrange
        var guid = Guid.Empty;

        // Act
        var result = _serializer.Serialize(guid);

        // Assert
        Assert.That(result, Is.EqualTo("00000000-0000-0000-0000-000000000000"));
    }

    [Test]
    public void Serialize_WithRandomGuid_ReturnsValidGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = _serializer.Serialize(guid);

        // Assert
        Assert.That(result, Is.TypeOf<string>());
        Assert.That(result.ToString().Length, Is.EqualTo(36)); // GUID format with hyphens
        Assert.DoesNotThrow(() => Guid.Parse(result.ToString()));
    }

    [Test]
    public void Deserialize_WithValidGuidString_ReturnsGuid()
    {
        // Arrange
        var guidString = "12345678-1234-1234-1234-123456789abc";
        var expectedGuid = new Guid(guidString);

        // Act
        var result = _serializer.Deserialize(guidString);

        // Assert
        Assert.That(result, Is.EqualTo(expectedGuid));
        Assert.That(result, Is.TypeOf<Guid>());
    }

    [Test]
    public void Deserialize_WithEmptyGuidString_ReturnsEmptyGuid()
    {
        // Arrange
        var guidString = "00000000-0000-0000-0000-000000000000";

        // Act
        var result = _serializer.Deserialize(guidString);

        // Assert
        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void Deserialize_WithInvalidGuidString_ThrowsFormatException()
    {
        // Arrange
        var invalidGuidString = "invalid-guid-string";

        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize(invalidGuidString));
    }

    [Test]
    public void Deserialize_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null));
    }

    [Test]
    public void SerializeDeserialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var originalGuid = Guid.NewGuid();

        // Act
        var serialized = _serializer.Serialize(originalGuid);
        var deserialized = _serializer.Deserialize(serialized);

        // Assert
        Assert.That(deserialized, Is.EqualTo(originalGuid));
    }

    [Test]
    public void Serialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a guid"));
    }
}