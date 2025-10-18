using LibSqlite3Orm;

namespace LibSqlite3Orm.UnitTests;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    public void UnicodeToUtf8_ValidString_ConvertsCorrectly()
    {
        // Arrange
        var input = "Hello World";

        // Act
        var result = input.UnicodeToUtf8();

        // Assert
        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void UnicodeToUtf8_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var input = "";

        // Act
        var result = input.UnicodeToUtf8();

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void UnicodeToUtf8_WithUnicodeCharacters_ConvertsCorrectly()
    {
        // Arrange
        var input = "Hello 世界";

        // Act
        var result = input.UnicodeToUtf8();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.Not.Empty);
    }

    [Test]
    public void Utf8ToUnicode_ValidString_ConvertsCorrectly()
    {
        // Arrange
        var input = "Hello World";

        // Act
        var result = input.Utf8ToUnicode();

        // Assert
        Assert.That(result, Is.EqualTo(input));
    }

    [Test]
    public void Utf8ToUnicode_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var input = "";

        // Act
        var result = input.Utf8ToUnicode();

        // Assert
        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void UnicodeToUtf8_ThenUtf8ToUnicode_ReturnsOriginalString()
    {
        // Arrange
        var original = "Test string with special chars: αβγδε";

        // Act
        var utf8 = original.UnicodeToUtf8();
        var backToUnicode = utf8.Utf8ToUnicode();

        // Assert
        Assert.That(backToUnicode, Is.EqualTo(original));
    }

    [Test]
    public void UnicodeToUtf8_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => input.UnicodeToUtf8());
    }

    [Test]
    public void Utf8ToUnicode_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string input = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => input.Utf8ToUnicode());
    }
}