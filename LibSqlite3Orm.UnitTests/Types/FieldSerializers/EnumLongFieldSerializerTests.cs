using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class EnumLongFieldSerializerTests
{
    private enum TestEnum
    {
        Value1,
        Value2,
        ValueWithSpecialName
    }

    private EnumLongFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new EnumLongFieldSerializer(typeof(TestEnum));
    }

    [Test]
    public void Constructor_WithValidEnumType_SetsEnumType()
    {
        // Assert
        Assert.That(_serializer.EnumType, Is.EqualTo(typeof(TestEnum)));
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(TestEnum)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(long)));
    }

    [Test]
    public void Serialize_WithEnumValue_ReturnsEnumNameString()
    {
        // Act
        var result = _serializer.Serialize(TestEnum.Value1);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(result, Is.TypeOf<long>());
    }

    [Test]
    public void Serialize_WithDifferentEnumValue_ReturnsCorrectName()
    {
        // Act
        var result = _serializer.Serialize(TestEnum.ValueWithSpecialName);

        // Assert
        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void Deserialize_WithValidEnumName_ReturnsEnumValue()
    {
        // Act
        var result = _serializer.Deserialize("Value1");

        // Assert
        Assert.That(result, Is.EqualTo(TestEnum.Value1));
        Assert.That(result, Is.TypeOf<TestEnum>());
    }

    [Test]
    public void Deserialize_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize(""));
    }

    [Test]
    public void SerializeDeserialize_RoundTrip_PreservesValue()
    {
        // Arrange
        var enumValues = Enum.GetValues<TestEnum>();

        foreach (var originalValue in enumValues)
        {
            // Act
            var serialized = _serializer.Serialize(originalValue);
            var deserialized = _serializer.Deserialize(serialized);

            // Assert
            Assert.That(deserialized, Is.EqualTo(originalValue), $"Failed for value {originalValue}");
        }
    }

    // Test with a different enum type
    private enum AnotherEnum
    {
        First = 1,
        Second = 2
    }

    [Test]
    public void DifferentEnumType_WorksCorrectly()
    {
        // Arrange
        var anotherSerializer = new EnumLongFieldSerializer(typeof(AnotherEnum));

        // Act
        var serialized = anotherSerializer.Serialize(AnotherEnum.First);
        var deserialized = anotherSerializer.Deserialize(1);

        // Assert
        Assert.That(serialized, Is.EqualTo(1));
        Assert.That(deserialized, Is.EqualTo(AnotherEnum.First));
        Assert.That(anotherSerializer.EnumType, Is.EqualTo(typeof(AnotherEnum)));
    }
}