using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class DecimalTextFieldSerializerTests
{
    private DecimalTextFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new DecimalTextFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsDecimalType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(decimal)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithWholeNumber_ReturnsFormattedString()
    {
        // Act
        var result = _serializer.Serialize(123m);

        // Assert
        Assert.That(result, Is.EqualTo("123.0"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Serialize_WithDecimalPlaces_ReturnsFormattedString()
    {
        // Act
        var result = _serializer.Serialize(123.456m);

        // Assert
        Assert.That(result, Is.EqualTo("123.456"));
    }

    [Test]
    public void Serialize_WithZero_ReturnsZeroString()
    {
        // Act
        var result = _serializer.Serialize(0m);

        // Assert
        Assert.That(result, Is.EqualTo("0.0"));
    }

    [Test]
    public void Serialize_WithNegativeNumber_ReturnsNegativeString()
    {
        // Act
        var result = _serializer.Serialize(-456.789m);

        // Assert
        Assert.That(result, Is.EqualTo("-456.789"));
    }

    [Test]
    public void Serialize_WithMaxValue_ReturnsMaxValueString()
    {
        // Act
        var result = _serializer.Serialize(decimal.MaxValue);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToString(), Does.Contain("79228162514264337593543950335"));
    }

    [Test]
    public void Serialize_WithMinValue_ReturnsMinValueString()
    {
        // Act
        var result = _serializer.Serialize(decimal.MinValue);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToString(), Does.Contain("-79228162514264337593543950335"));
    }

    [Test]
    public void Serialize_WithHighPrecision_ReturnsFullPrecisionString()
    {
        // Arrange
        var highPrecisionDecimal = 1.123456789012345678901234567890m;

        // Act
        var result = _serializer.Serialize(highPrecisionDecimal);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ToString(), Does.StartWith("1.1234567890123456789012345679"));
    }

    [Test]
    public void Deserialize_WithValidDecimalString_ReturnsDecimal()
    {
        // Act
        var result = _serializer.Deserialize("123.456");

        // Assert
        Assert.That(result, Is.EqualTo(123.456m));
        Assert.That(result, Is.TypeOf<decimal>());
    }

    [Test]
    public void Deserialize_WithWholeNumberString_ReturnsDecimal()
    {
        // Act
        var result = _serializer.Deserialize("789");

        // Assert
        Assert.That(result, Is.EqualTo(789m));
    }

    [Test]
    public void Deserialize_WithZeroString_ReturnsZero()
    {
        // Act
        var result = _serializer.Deserialize("0");

        // Assert
        Assert.That(result, Is.EqualTo(0m));
    }

    [Test]
    public void Deserialize_WithNegativeString_ReturnsNegativeDecimal()
    {
        // Act
        var result = _serializer.Deserialize("-123.456");

        // Assert
        Assert.That(result, Is.EqualTo(-123.456m));
    }

    [Test]
    public void Deserialize_WithInvalidString_ThrowsFormatException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize("not a decimal"));
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
        var testValues = new[] { 0m, 1m, -1m, 123.456m, -789.123m, 0.1m, 999999.999999m };

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
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a decimal"));
    }

    [Test]
    public void Deserialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Deserialize(123));
    }
}