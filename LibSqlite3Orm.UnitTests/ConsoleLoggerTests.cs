namespace LibSqlite3Orm.UnitTests;

[TestFixture]
public class ConsoleLoggerTests
{
    private StringWriter _stringWriter;
    private TextWriter _originalConsoleOut;
    private ConsoleColor _originalConsoleColor;

    [SetUp]
    public void SetUp()
    {
        _stringWriter = new StringWriter();
        _originalConsoleOut = Console.Out;
        _originalConsoleColor = Console.ForegroundColor;
        Console.SetOut(_stringWriter);
    }

    [TearDown]
    public void TearDown()
    {
        Console.SetOut(_originalConsoleOut);
        Console.ForegroundColor = _originalConsoleColor;
        _stringWriter?.Dispose();
    }

    [Test]
    public void WriteLine_WithMessage_WritesToConsole()
    {
        // Arrange
        var message = "Test message";

        // Act
        ConsoleLogger.WriteLine(message);

        // Assert
        var output = _stringWriter.ToString();
        Assert.That(output, Contains.Substring(message));
    }

    [Test]
    public void WriteLine_WithColorAndMessage_WritesToConsoleWithColor()
    {
        // Arrange
        var message = "Test message with color";
        var color = ConsoleColor.Red;

        // Act
        ConsoleLogger.WriteLine(color, message);

        // Assert
        var output = _stringWriter.ToString();
        Assert.That(output, Contains.Substring(message));
    }

    [Test]
    public void WriteLine_NullColor_WritesWithoutColor()
    {
        // Arrange
        var message = "Message without color";

        // Act
        ConsoleLogger.WriteLine((ConsoleColor?)null, message);

        // Assert
        var output = _stringWriter.ToString();
        Assert.That(output, Contains.Substring(message));
    }
}