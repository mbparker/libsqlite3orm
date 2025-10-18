using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class CharTextFieldSerializerTests
{
    private CharTextFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new CharTextFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsCharType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(char)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithRegularChar_ReturnsString()
    {
        // Act
        var result = _serializer.Serialize('A');

        // Assert
        Assert.That(result, Is.EqualTo("A"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Serialize_WithSpace_ReturnsSpaceString()
    {
        // Act
        var result = _serializer.Serialize(' ');

        // Assert
        Assert.That(result, Is.EqualTo(" "));
    }

    [Test]
    public void Serialize_WithDigit_ReturnsDigitString()
    {
        // Act
        var result = _serializer.Serialize('9');

        // Assert
        Assert.That(result, Is.EqualTo("9"));
    }

    [Test]
    public void Serialize_WithSpecialChar_ReturnsSpecialCharString()
    {
        // Act
        var result = _serializer.Serialize('!');

        // Assert
        Assert.That(result, Is.EqualTo("!"));
    }

    [Test]
    public void Deserialize_WithSingleCharString_ReturnsChar()
    {
        // Act
        var result = _serializer.Deserialize("X");

        // Assert
        Assert.That(result, Is.EqualTo('X'));
        Assert.That(result, Is.TypeOf<char>());
    }

    [Test]
    public void Deserialize_WithMultiCharString_ReturnsFirstChar()
    {
        // Act
        var result = _serializer.Deserialize("Hello");

        // Assert
        Assert.That(result, Is.EqualTo('H'));
    }

    [Test]
    public void Deserialize_WithEmptyString_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _serializer.Deserialize(""));
    }

    [Test]
    public void Deserialize_WithNullString_ThrowsNullReferenceException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _serializer.Deserialize(null));
    }

    [Test]
    public void SerializeDeserialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var originalChars = new[] { 'A', 'z', '5', '@', ' ', '\n', '\t' };

        foreach (var originalChar in originalChars)
        {
            // Act
            var serialized = _serializer.Serialize(originalChar);
            var deserialized = _serializer.Deserialize(serialized);

            // Assert
            Assert.That(deserialized, Is.EqualTo(originalChar), $"Failed for char '{originalChar}'");
        }
    }

    [Test]
    public void Serialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a char"));
    }

    [Test]
    public void Deserialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Deserialize(123));
    }
}