using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class TimeOnlyTextFieldSerializerTests
{
    private TimeOnlyTextFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new TimeOnlyTextFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsTimeOnlyType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(TimeOnly)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithMidnightTime_ReturnsFormattedString()
    {
        // Arrange
        var time = new TimeOnly(0, 0, 0);

        // Act
        var result = _serializer.Serialize(time);

        // Assert
        Assert.That(result, Is.EqualTo("00:00:00.0000000"));
    }

    [Test]
    public void Serialize_WithNoonTime_ReturnsFormattedString()
    {
        // Arrange
        var time = new TimeOnly(12, 0, 0);

        // Act
        var result = _serializer.Serialize(time);

        // Assert
        Assert.That(result, Is.EqualTo("12:00:00.0000000"));
    }

    [Test]
    public void Serialize_WithSpecificTime_ReturnsFormattedString()
    {
        // Arrange
        var time = new TimeOnly(14, 30, 45, 123);

        // Act
        var result = _serializer.Serialize(time);

        // Assert
        Assert.That(result, Does.StartWith("14:30:45.1230000"));
    }

    [Test]
    public void Serialize_WithMaxValue_ReturnsMaxValueString()
    {
        // Act
        var result = _serializer.Serialize(TimeOnly.MaxValue);

        // Assert
        Assert.That(result, Is.EqualTo("23:59:59.9999999"));
    }

    [Test]
    public void Serialize_WithMinValue_ReturnsMinValueString()
    {
        // Act
        var result = _serializer.Serialize(TimeOnly.MinValue);

        // Assert
        Assert.That(result, Is.EqualTo("00:00:00.0000000"));
    }

    [Test]
    public void Deserialize_WithValidTimeString_ReturnsTimeOnly()
    {
        // Act
        var result = _serializer.Deserialize("14:30:45.1230000");

        // Assert
        Assert.That(result, Is.EqualTo(new TimeOnly(14, 30, 45, 123)));
        Assert.That(result, Is.TypeOf<TimeOnly>());
    }

    [Test]
    public void Deserialize_WithMidnightString_ReturnsMidnight()
    {
        // Act
        var result = _serializer.Deserialize("00:00:00.0000000");

        // Assert
        Assert.That(result, Is.EqualTo(new TimeOnly(0, 0, 0)));
    }

    [Test]
    public void Deserialize_WithMaxValueString_ReturnsMaxValue()
    {
        // Act
        var result = _serializer.Deserialize("23:59:59.9999999");

        // Assert
        Assert.That(result, Is.EqualTo(TimeOnly.MaxValue));
    }

    [Test]
    public void Deserialize_WithInvalidFormat_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize("not a time"));
    }

    [Test]
    public void Deserialize_WithInvalidTime_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize("25:00:00.0000000"));
    }

    [Test]
    public void Deserialize_WithNullString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null));
    }

    [Test]
    public void Deserialize_WithEmptyString_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize(""));
    }

    [Test]
    public void SerializeDeserialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var testValues = new[]
        {
            TimeOnly.MinValue,
            TimeOnly.MaxValue,
            new TimeOnly(12, 0, 0),
            new TimeOnly(14, 30, 45, 123),
            new TimeOnly(23, 59, 59, 999)
        };

        foreach (var originalValue in testValues)
        {
            // Act
            var serialized = _serializer.Serialize(originalValue);
            var deserialized = _serializer.Deserialize(serialized);

            // Assert
            Assert.That(deserialized, Is.EqualTo(originalValue), $"Failed for value {originalValue}");
        }
    }

    [Test]
    public void Serialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a time"));
    }
}