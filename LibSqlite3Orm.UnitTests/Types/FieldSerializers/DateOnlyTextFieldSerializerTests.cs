using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm.UnitTests.Types.FieldSerializers;

[TestFixture]
public class DateOnlyTextFieldSerializerTests
{
    private DateOnlyTextFieldSerializer _serializer;

    [SetUp]
    public void SetUp()
    {
        _serializer = new DateOnlyTextFieldSerializer();
    }

    [Test]
    public void RuntimeType_ReturnsDateOnlyType()
    {
        // Act & Assert
        Assert.That(_serializer.RuntimeType, Is.EqualTo(typeof(DateOnly)));
    }

    [Test]
    public void SerializedType_ReturnsStringType()
    {
        // Act & Assert
        Assert.That(_serializer.SerializedType, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void Serialize_WithValidDate_ReturnsFormattedString()
    {
        // Arrange
        var date = new DateOnly(2023, 12, 25);

        // Act
        var result = _serializer.Serialize(date);

        // Assert
        Assert.That(result, Is.EqualTo("2023-12-25"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Serialize_WithMinDate_ReturnsFormattedString()
    {
        // Arrange
        var date = DateOnly.MinValue;

        // Act
        var result = _serializer.Serialize(date);

        // Assert
        Assert.That(result, Is.EqualTo("0001-01-01"));
    }

    [Test]
    public void Serialize_WithMaxDate_ReturnsFormattedString()
    {
        // Arrange
        var date = DateOnly.MaxValue;

        // Act
        var result = _serializer.Serialize(date);

        // Assert
        Assert.That(result, Is.EqualTo("9999-12-31"));
    }

    [Test]
    public void Serialize_WithLeapYearDate_ReturnsFormattedString()
    {
        // Arrange
        var date = new DateOnly(2020, 2, 29); // Leap year

        // Act
        var result = _serializer.Serialize(date);

        // Assert
        Assert.That(result, Is.EqualTo("2020-02-29"));
    }

    [Test]
    public void Deserialize_WithValidDateString_ReturnsDateOnly()
    {
        // Arrange
        var dateString = "2023-12-25";
        var expectedDate = new DateOnly(2023, 12, 25);

        // Act
        var result = _serializer.Deserialize(dateString);

        // Assert
        Assert.That(result, Is.EqualTo(expectedDate));
        Assert.That(result, Is.TypeOf<DateOnly>());
    }

    [Test]
    public void Deserialize_WithMinDateString_ReturnsMinDate()
    {
        // Arrange
        var dateString = "0001-01-01";

        // Act
        var result = _serializer.Deserialize(dateString);

        // Assert
        Assert.That(result, Is.EqualTo(DateOnly.MinValue));
    }

    [Test]
    public void Deserialize_WithMaxDateString_ReturnsMaxDate()
    {
        // Arrange
        var dateString = "9999-12-31";

        // Act
        var result = _serializer.Deserialize(dateString);

        // Assert
        Assert.That(result, Is.EqualTo(DateOnly.MaxValue));
    }

    [Test]
    public void Deserialize_WithLeapYearDateString_ReturnsDateOnly()
    {
        // Arrange
        var dateString = "2020-02-29";
        var expectedDate = new DateOnly(2020, 2, 29);

        // Act
        var result = _serializer.Deserialize(dateString);

        // Assert
        Assert.That(result, Is.EqualTo(expectedDate));
    }

    [Test]
    public void Deserialize_WithInvalidFormat_ThrowsFormatException()
    {
        // Arrange
        var invalidDateString = "25-12-2023"; // Wrong format

        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize(invalidDateString));
    }

    [Test]
    public void Deserialize_WithInvalidDate_ThrowsFormatException()
    {
        // Arrange
        var invalidDateString = "2023-13-01"; // Invalid month

        // Act & Assert
        Assert.Throws<FormatException>(() => _serializer.Deserialize(invalidDateString));
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
        var originalDate = new DateOnly(2023, 6, 15);

        // Act
        var serialized = _serializer.Serialize(originalDate);
        var deserialized = _serializer.Deserialize(serialized);

        // Assert
        Assert.That(deserialized, Is.EqualTo(originalDate));
    }

    [Test]
    public void Serialize_WithInvalidType_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize("not a date"));
    }

    [Test]
    public void Serialize_WithDateTime_ThrowsInvalidCastException()
    {
        // Act & Assert
        Assert.Throws<InvalidCastException>(() => _serializer.Serialize(DateTime.Now));
    }
}