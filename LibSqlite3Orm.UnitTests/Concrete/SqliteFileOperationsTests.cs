using LibSqlite3Orm.Concrete;

namespace LibSqlite3Orm.UnitTests.Concrete;

[TestFixture]
public class SqliteFileOperationsTests
{
    private SqliteFileOperations _fileOperations;
    private string _testFilePath;

    [SetUp]
    public void SetUp()
    {
        _fileOperations = new SqliteFileOperations();
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
        {
            try
            {
                File.Delete(_testFilePath);
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }
    }

    [Test]
    public void FileExists_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "test content");

        // Act
        var result = _fileOperations.FileExists(_testFilePath);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void FileExists_WithNonExistentFile_ReturnsFalse()
    {
        // Act
        var result = _fileOperations.FileExists(_testFilePath);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void FileExists_WithEmptyPath_ReturnsFalse()
    {
        // Act
        var result = _fileOperations.FileExists("");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void FileExists_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _fileOperations.FileExists(null));
    }

    [Test]
    public void DeleteFile_WithExistingFile_DeletesFile()
    {
        // Arrange
        File.WriteAllText(_testFilePath, "test content");
        Assert.That(File.Exists(_testFilePath), Is.True, "Setup: File should exist");

        // Act
        _fileOperations.DeleteFile(_testFilePath);

        // Assert
        Assert.That(File.Exists(_testFilePath), Is.False, "File should be deleted");
    }

    [Test]
    public void DeleteFile_WithNonExistentFile_DoesNotThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _fileOperations.DeleteFile(_testFilePath));
    }

    [Test]
    public void DeleteFile_WithNullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _fileOperations.DeleteFile(null));
    }

    [Test]
    public void DeleteFile_WithEmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _fileOperations.DeleteFile(""));
    }

    [Test]
    public void DeleteFile_WithInvalidPath_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _fileOperations.DeleteFile("\\|<>:?*"));
    }
}