using LibSqlite3Orm.Types.Orm.FieldConverters;

namespace LibSqlite3Orm.UnitTests.Types.Orm.FieldConverters;

[TestFixture]
public class IntegralFieldConverterTests
{
    private IntegralFieldConverter _converter;

    [SetUp]
    public void SetUp()
    {
        _converter = new IntegralFieldConverter();
    }

    [Test]
    public void CanConvert_WithConvertibleTypes_ReturnsTrue()
    {
        // Act & Assert
        Assert.That(_converter.CanConvert(typeof(int), typeof(long)), Is.True);
        Assert.That(_converter.CanConvert(typeof(short), typeof(int)), Is.True);
        Assert.That(_converter.CanConvert(typeof(byte), typeof(int)), Is.True);
        Assert.That(_converter.CanConvert(typeof(float), typeof(double)), Is.True);
        Assert.That(_converter.CanConvert(typeof(int), typeof(string)), Is.True);
        Assert.That(_converter.CanConvert(typeof(double), typeof(decimal)), Is.True);
    }

    [Test]
    public void CanConvert_WithNullableTypes_ReturnsTrue()
    {
        // Act & Assert
        Assert.That(_converter.CanConvert(typeof(int?), typeof(long)), Is.True);
        Assert.That(_converter.CanConvert(typeof(int), typeof(long?)), Is.True);
        Assert.That(_converter.CanConvert(typeof(int?), typeof(long?)), Is.True);
    }

    [Test]
    public void CanConvert_WithSameTypes_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(_converter.CanConvert(typeof(int), typeof(int)), Is.False);
        Assert.That(_converter.CanConvert(typeof(string), typeof(string)), Is.False);
        Assert.That(_converter.CanConvert(typeof(int?), typeof(int?)), Is.False);
    }

    [Test]
    public void CanConvert_WithObjectType_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(_converter.CanConvert(typeof(object), typeof(int)), Is.False);
        Assert.That(_converter.CanConvert(typeof(int), typeof(object)), Is.False);
        Assert.That(_converter.CanConvert(typeof(object), typeof(object)), Is.False);
    }

    [Test]
    public void CanConvert_WithNonConvertibleTypes_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(_converter.CanConvert(typeof(List<int>), typeof(int)), Is.False);
        Assert.That(_converter.CanConvert(typeof(int), typeof(List<int>)), Is.False);
    }

    [Test]
    public void Convert_IntToLong_ReturnsLong()
    {
        // Arrange
        int value = 42;

        // Act
        var result = _converter.Convert(typeof(int), value, typeof(long));

        // Assert
        Assert.That(result, Is.EqualTo(42L));
        Assert.That(result, Is.TypeOf<long>());
    }

    [Test]
    public void Convert_FloatToDouble_ReturnsDouble()
    {
        // Arrange
        float value = 3.14f;

        // Act
        var result = _converter.Convert(typeof(float), value, typeof(double));

        // Assert
        Assert.That(result, Is.EqualTo(3.14d).Within(0.001));
        Assert.That(result, Is.TypeOf<double>());
    }

    [Test]
    public void Convert_IntToString_ReturnsString()
    {
        // Arrange
        int value = 123;

        // Act
        var result = _converter.Convert(typeof(int), value, typeof(string));

        // Assert
        Assert.That(result, Is.EqualTo("123"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Convert_StringToInt_ReturnsInt()
    {
        // Arrange
        string value = "456";

        // Act
        var result = _converter.Convert(typeof(string), value, typeof(int));

        // Assert
        Assert.That(result, Is.EqualTo(456));
        Assert.That(result, Is.TypeOf<int>());
    }

    [Test]
    public void Convert_NullValueToValueType_ReturnsDefaultValue()
    {
        // Act
        var result = _converter.Convert(typeof(int?), null, typeof(uint));

        // Assert
        Assert.That(result, Is.EqualTo(0));
        Assert.That(result, Is.TypeOf<uint>());
    }

    [Test]
    public void Convert_NullValueToReferenceType_ReturnsNull()
    {
        // Act
        var result = _converter.Convert(typeof(int?), null, typeof(long?));

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Convert_NullableIntToLong_ReturnsLong()
    {
        // Arrange
        int? value = 42;

        // Act
        var result = _converter.Convert(typeof(int?), value, typeof(long));

        // Assert
        Assert.That(result, Is.EqualTo(42L));
        Assert.That(result, Is.TypeOf<long>());
    }

    [Test]
    public void Convert_IntToNullableLong_ReturnsNullableLong()
    {
        // Arrange
        int value = 42;

        // Act
        var result = _converter.Convert(typeof(int), value, typeof(long?));

        // Assert
        Assert.That(result, Is.EqualTo(42L));
        Assert.That(result, Is.TypeOf<long>());
    }
    
    [Test]
    public void Convert_WithSameUnderlyingTypes_ThrowsNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => 
            _converter.Convert(typeof(int?), 42, typeof(int)));
    }

    [Test]
    public void Convert_WithUnsupportedConversion_ThrowsNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() => 
            _converter.Convert(typeof(int), 42, typeof(int)));
    }

    [Test]
    public void Convert_WithInvalidValueForConversion_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => 
            _converter.Convert(typeof(string), "not a number", typeof(int)));
    }

    [Test]
    public void Convert_WithFormatProvider_UsesProvider()
    {
        // Arrange
        var formatProvider = System.Globalization.CultureInfo.InvariantCulture;
        double value = 3.14;

        // Act
        var result = _converter.Convert(typeof(double), value, typeof(string), formatProvider);

        // Assert
        Assert.That(result, Is.EqualTo("3.14"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Convert_ByteToInt_ReturnsInt()
    {
        // Arrange
        byte value = 255;

        // Act
        var result = _converter.Convert(typeof(byte), value, typeof(int));

        // Assert
        Assert.That(result, Is.EqualTo(255));
        Assert.That(result, Is.TypeOf<int>());
    }

    [Test]
    public void Convert_IntToByte_WithOverflow_ThrowsOverflowException()
    {
        // Arrange
        int value = 300; // Larger than byte max value

        // Act & Assert
        Assert.Throws<OverflowException>(() => 
            _converter.Convert(typeof(int), value, typeof(byte)));
    }

    [Test]
    public void Convert_BooleanToString_ReturnsString()
    {
        // Arrange
        bool value = true;

        // Act
        var result = _converter.Convert(typeof(bool), value, typeof(string));

        // Assert
        Assert.That(result, Is.EqualTo("True"));
        Assert.That(result, Is.TypeOf<string>());
    }

    [Test]
    public void Convert_StringToBoolean_ReturnsBoolean()
    {
        // Act & Assert
        var result1 = _converter.Convert(typeof(string), "True", typeof(bool));
        Assert.That(result1, Is.EqualTo(true));

        var result2 = _converter.Convert(typeof(string), "False", typeof(bool));
        Assert.That(result2, Is.EqualTo(false));
    }
}