using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Concrete;

namespace LibSqlite3Orm.UnitTests.Concrete;

[TestFixture]
public class SqliteFieldValueSerializationTests
{
    private SqliteFieldValueSerialization _serialization;
    private ISqliteFieldSerializer _mockStringSerializer;
    private ISqliteFieldSerializer _mockIntSerializer;
    private Func<Type, ISqliteEnumFieldSerializer> _mockEnumFactory;
    private ISqliteEnumFieldSerializer _mockEnumSerializer;

    private enum TestEnum
    {
        Value1,
        Value2
    }

    [SetUp]
    public void SetUp()
    {
        _mockStringSerializer = Substitute.For<ISqliteFieldSerializer>();
        _mockStringSerializer.RuntimeType.Returns(typeof(string));

        _mockIntSerializer = Substitute.For<ISqliteFieldSerializer>();
        _mockIntSerializer.RuntimeType.Returns(typeof(int));

        _mockEnumSerializer = Substitute.For<ISqliteEnumFieldSerializer>();
        _mockEnumSerializer.RuntimeType.Returns(typeof(TestEnum));

        _mockEnumFactory = Substitute.For<Func<Type, ISqliteEnumFieldSerializer>>();
        _mockEnumFactory.Invoke(Arg.Any<Type>()).Returns(_mockEnumSerializer);

        var serializers = new[] { _mockStringSerializer, _mockIntSerializer };
        _serialization = new SqliteFieldValueSerialization(serializers, _mockEnumFactory);
    }

    [Test]
    public void Constructor_WithSerializers_InitializesCorrectly()
    {
        // Assert
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(string)), Is.True);
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(int)), Is.True);
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(decimal)), Is.False);
    }

    [Test]
    public void IsSerializerRegisteredForModelType_WithRegisteredType_ReturnsTrue()
    {
        // Act & Assert
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(string)), Is.True);
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(int)), Is.True);
    }

    [Test]
    public void IsSerializerRegisteredForModelType_WithUnregisteredType_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(decimal)), Is.False);
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(bool)), Is.False);
    }

    [Test]
    public void Indexer_WithRegisteredType_ReturnsCorrectSerializer()
    {
        // Act
        var stringSerializer = _serialization[typeof(string)];
        var intSerializer = _serialization[typeof(int)];

        // Assert
        Assert.That(stringSerializer, Is.EqualTo(_mockStringSerializer));
        Assert.That(intSerializer, Is.EqualTo(_mockIntSerializer));
    }

    [Test]
    public void Indexer_WithUnregisteredType_ReturnsNull()
    {
        // Act
        var result = _serialization[typeof(decimal)];

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Indexer_WithEnumType_CreatesAndReturnsEnumSerializer()
    {
        // Act
        var result = _serialization[typeof(TestEnum)];

        // Assert
        Assert.That(result, Is.EqualTo(_mockEnumSerializer));
        _mockEnumFactory.Received(1).Invoke(typeof(TestEnum));
        
        // Verify it's cached for subsequent calls
        var result2 = _serialization[typeof(TestEnum)];
        Assert.That(result2, Is.EqualTo(_mockEnumSerializer));
        _mockEnumFactory.Received(1).Invoke(typeof(TestEnum)); // Should still be called only once
    }

    [Test]
    public void RegisterSerializer_WithNewType_AddsSerializer()
    {
        // Arrange
        var mockDecimalSerializer = Substitute.For<ISqliteFieldSerializer>();
        mockDecimalSerializer.RuntimeType.Returns(typeof(decimal));

        // Act
        _serialization.RegisterSerializer(mockDecimalSerializer);

        // Assert
        Assert.That(_serialization.IsSerializerRegisteredForModelType(typeof(decimal)), Is.True);
        Assert.That(_serialization[typeof(decimal)], Is.EqualTo(mockDecimalSerializer));
    }

    [Test]
    public void RegisterSerializer_WithExistingType_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockStringSerializer2 = Substitute.For<ISqliteFieldSerializer>();
        mockStringSerializer2.RuntimeType.Returns(typeof(string));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _serialization.RegisterSerializer(mockStringSerializer2));
    }

    [Test]
    public void ReplaceSerializer_WithExistingType_ReplacesSerializer()
    {
        // Arrange
        var newStringSerializer = Substitute.For<ISqliteFieldSerializer>();
        newStringSerializer.RuntimeType.Returns(typeof(string));

        // Act
        _serialization.ReplaceSerializer(typeof(string), newStringSerializer);

        // Assert
        Assert.That(_serialization[typeof(string)], Is.EqualTo(newStringSerializer));
    }

    [Test]
    public void ReplaceSerializer_WithNonExistentType_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockDecimalSerializer = Substitute.For<ISqliteFieldSerializer>();
        mockDecimalSerializer.RuntimeType.Returns(typeof(decimal));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _serialization.ReplaceSerializer(typeof(decimal), mockDecimalSerializer));
    }

    [Test]
    public void ReplaceSerializer_WithMismatchedType_ThrowsArgumentException()
    {
        // Arrange
        var mockDecimalSerializer = Substitute.For<ISqliteFieldSerializer>();
        mockDecimalSerializer.RuntimeType.Returns(typeof(decimal));

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serialization.ReplaceSerializer(typeof(string), mockDecimalSerializer));
    }

    [Test]
    public void ReplaceSerializer_WithDisposableOldSerializer_DisposesOldSerializer()
    {
        // Arrange
        var disposableSerializer = Substitute.For<ISqliteFieldSerializer, IDisposable>();
        disposableSerializer.RuntimeType.Returns(typeof(float));
        _serialization.RegisterSerializer(disposableSerializer);

        var newSerializer = Substitute.For<ISqliteFieldSerializer>();
        newSerializer.RuntimeType.Returns(typeof(float));

        // Act
        _serialization.ReplaceSerializer(typeof(float), newSerializer);

        // Assert
        ((IDisposable)disposableSerializer).Received(1).Dispose();
    }

    [Test]
    public void ThreadSafety_ConcurrentAccess_HandledCorrectly()
    {
        // This is a basic test for thread safety - in a real scenario you might want more comprehensive testing
        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    var serializer = Substitute.For<ISqliteFieldSerializer>();
                    serializer.RuntimeType.Returns(typeof(List<>).MakeGenericType(typeof(int)));

                    if (index % 2 == 0)
                    {
                        _ = _serialization.IsSerializerRegisteredForModelType(typeof(string));
                    }
                    else
                    {
                        _ = _serialization[typeof(int)];
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        // Act & Assert
        Task.WaitAll(tasks.ToArray());
        Assert.That(exceptions, Is.Empty, "No exceptions should occur during concurrent access");
    }

    [Test]
    public void Constructor_WithEmptySerializers_InitializesEmpty()
    {
        // Arrange & Act
        var emptySerialization = new SqliteFieldValueSerialization(Array.Empty<ISqliteFieldSerializer>(), _mockEnumFactory);

        // Assert
        Assert.That(emptySerialization.IsSerializerRegisteredForModelType(typeof(string)), Is.False);
        Assert.That(emptySerialization[typeof(string)], Is.Null);
    }
}