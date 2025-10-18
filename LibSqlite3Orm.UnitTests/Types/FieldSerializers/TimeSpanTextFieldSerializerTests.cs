using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class TimeSpanTextFieldSerializerTests
{
    private TimeSpanTextFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new TimeSpanTextFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsTimeSpanType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(TimeSpan)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithZeroTimeSpan_ReturnsFormattedString()
    {
        // Act
        var result = _serializer.Serialize(TimeSpan.Zero);

        // Assert
        Assert.That(result, Is.EqualTo("00:00:00"));
    }

    [Test]
    public void Serialize_WithPositiveTimeSpan_ReturnsFormattedString()
    {
        // Arrange
        var timeSpan = new TimeSpan(2, 14, 30, 45, 123);

        // Act
        var result = _serializer.Serialize(timeSpan);

        // Assert
        Assert.That(result, Does.StartWith("2.14:30:45.123"));
    }

    [Test]
    public void Serialize_WithNegativeTimeSpan_ReturnsFormattedString()
    {
        // Arrange
        var timeSpan = new TimeSpan(-1, -2, -3, -4, -5);

        // Act
        var result = _serializer.Serialize(timeSpan);

        // Assert
        Assert.That(result, Does.StartWith("-1.02:03:04.005"));
    }

    [Test]
    public void Serialize_WithMaxValue_ReturnsMaxValueString()
    {
        // Act
        var result = _serializer.Serialize(TimeSpan.MaxValue);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToString(), Does.Contain("10675199"));
    }

    [Test]
    public void Serialize_WithMinValue_ReturnsMinValueString()
    {
        // Act
        var result = _serializer.Serialize(TimeSpan.MinValue);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToString(), Does.Contain("-10675199"));
    }

    [Test]
    public void Deserialize_WithValidTimeSpanString_ReturnsTimeSpan()
    {
        // Act
        var result = _serializer.Deserialize("1.12:30:45.1230000");

        // Assert
        Assert.That(result, Is.EqualTo(new TimeSpan(1, 12, 30, 45, 123)));
        Assert.That(result, Is.TypeOf<TimeSpan>());
    }

    [Test]
    public void Deserialize_WithZeroString_ReturnsZeroTimeSpan()
    {
        // Act
        var result = _serializer.Deserialize("0.00:00:00.0000000");

        // Assert
        Assert.That(result, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void Deserialize_WithNegativeString_ReturnsNegativeTimeSpan()
    {
        // Act
        var result = _serializer.Deserialize("-1.12:30:45.1230000");

        // Assert
        Assert.That(result, Is.EqualTo(new TimeSpan(-1, -12, -30, -45, -123)));
    }

    [Test]
    public void Deserialize_WithInvalidFormat_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize("not a timespan"));
    }

    [Test]
    public void Deserialize_WithInvalidTimeSpan_ThrowsOverflowException()
    {
        // Act & Assert
        Assert.Throws<OverflowException>(() => _serializer.Deserialize("25:61:61"));
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
            TimeSpan.Zero,
            new TimeSpan(1, 2, 3),
            new TimeSpan(5, 14, 30, 45, 123),
            new TimeSpan(-2, -10, -5, -30, -456)
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
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a timespan"));
    }
}