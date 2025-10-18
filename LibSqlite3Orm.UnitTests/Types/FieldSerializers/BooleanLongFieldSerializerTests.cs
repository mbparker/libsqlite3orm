using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class BooleanLongFieldSerializerTests
{
    private BooleanLongFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new BooleanLongFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsBooleanType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(bool)));
    }

    [Test]
    public void SerializedType_ReturnsLongType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(long)));
    }

    [Test]
    public void Serialize_WithTrue_ReturnsOne()
    {
        // Act
        var result = _serializer.Serialize(true);

        // Assert
        Assert.That(result, Is.EqualTo(1L));
        Assert.That(result, Is.TypeOf<long>());
    }

    [Test]
    public void Serialize_WithFalse_ReturnsZero()
    {
        // Act
        var result = _serializer.Serialize(false);

        // Assert
        Assert.That(result, Is.EqualTo(0L));
        Assert.That(result, Is.TypeOf<long>());
    }

    [Test]
    public void Deserialize_WithZero_ReturnsFalse()
    {
        // Act
        var result = _serializer.Deserialize(0L);

        // Assert
        Assert.That(result, Is.EqualTo(false));
        Assert.That(result, Is.TypeOf<bool>());
    }

    [Test]
    public void Deserialize_WithOne_ReturnsTrue()
    {
        // Act
        var result = _serializer.Deserialize(1L);

        // Assert
        Assert.That(result, Is.EqualTo(true));
        Assert.That(result, Is.TypeOf<bool>());
    }

    [Test]
    public void Deserialize_WithPositiveNumber_ReturnsTrue()
    {
        // Act
        var result = _serializer.Deserialize(42L);

        // Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void Deserialize_WithNegativeNumber_ReturnsTrue()
    {
        // Act
        var result = _serializer.Deserialize(-1L);

        // Assert
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public void SerializeDeserialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var originalTrue = true;
        var originalFalse = false;

        // Act
        var serializedTrue = _serializer.Serialize(originalTrue);
        var deserializedTrue = _serializer.Deserialize(serializedTrue);

        var serializedFalse = _serializer.Serialize(originalFalse);
        var deserializedFalse = _serializer.Deserialize(serializedFalse);

        // Assert
        Assert.That(deserializedTrue, Is.EqualTo(originalTrue));
        Assert.That(deserializedFalse, Is.EqualTo(originalFalse));
    }

    [Test]
    public void Serialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a boolean"));
    }

    [Test]
    public void Deserialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Deserialize("not a long"));
    }
}