using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Concrete;

namespace LibSqlite3Orm.UnitTests.Concrete;

[TestFixture]
public class SqliteParameterCollectionTests
{
    private SqliteParameterCollection _collection;
    private ISqliteParameter _mockParameter;

    [SetUp]
    public void SetUp()
    {
        _mockParameter = Substitute.For<ISqliteParameter>();
        var parameterFactory = Substitute.For<Func<string, int, ISqliteParameter>>();
        parameterFactory.Invoke(Arg.Any<string>(), Arg.Any<int>()).Returns(_mockParameter);
        
        _collection = new SqliteParameterCollection(parameterFactory);
    }

    [Test]
    public void Count_InitiallyEmpty_ReturnsZero()
    {
        // Assert
        Assert.That(_collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void Add_WithNameAndValue_ReturnsParameter()
    {
        // Arrange
        var name = "param1";
        var value = "test";

        // Act
        var result = _collection.Add(name, value);

        // Assert
        Assert.That(result, Is.EqualTo(_mockParameter));
        Assert.That(_collection.Count, Is.EqualTo(1));
        _mockParameter.Received(1).Set(value);
    }

    [Test]
    public void IndexerByInt_WithValidIndex_ReturnsParameter()
    {
        // Arrange
        _collection.Add("param1", "value1");

        // Act
        var result = _collection[0];

        // Assert
        Assert.That(result, Is.EqualTo(_mockParameter));
    }

    [Test]
    public void IndexerByString_WithExistingName_ReturnsParameter()
    {
        // Arrange
        var paramName = "testParam";
        _mockParameter.Name.Returns(paramName);
        _collection.Add(paramName, "value1");

        // Act
        var result = _collection[paramName];

        // Assert
        Assert.That(result, Is.EqualTo(_mockParameter));
    }

    [Test]
    public void IndexerByString_WithNonExistingName_ReturnsNull()
    {
        // Arrange
        _mockParameter.Name.Returns("param1");
        _collection.Add("param1", "value1");

        // Act
        var result = _collection["nonexistent"];

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void BindAll_CallsBindOnAllParameters()
    {
        // Arrange
        var statement = new IntPtr(123);
        _collection.Add("param1", "value1");
        _collection.Add("param2", "value2");

        // Act
        _collection.BindAll(statement);

        // Assert
        _mockParameter.Received(2).Bind(statement);
    }

    [Test]
    public void GetEnumerator_Generic_ReturnsAllParameters()
    {
        // Arrange
        _collection.Add("param1", "value1");
        _collection.Add("param2", "value2");

        // Act
        var parameters = _collection.ToList<ISqliteParameter>();

        // Assert
        Assert.That(parameters.Count, Is.EqualTo(2));
        Assert.That(parameters.All(p => p == _mockParameter), Is.True);
    }

    [Test]
    public void GetEnumerator_NonGeneric_ReturnsAllParameters()
    {
        // Arrange
        _collection.Add("param1", "value1");
        System.Collections.IEnumerable enumerable = _collection;

        // Act
        var count = 0;
        foreach (var item in enumerable)
        {
            count++;
            Assert.That(item, Is.EqualTo(_mockParameter));
        }

        // Assert
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void ParameterFactory_CalledWithCorrectIndex()
    {
        // Arrange
        var parameterFactory = Substitute.For<Func<string, int, ISqliteParameter>>();
        parameterFactory.Invoke(Arg.Any<string>(), Arg.Any<int>()).Returns(_mockParameter);
        var collection = new SqliteParameterCollection(parameterFactory);

        // Act
        collection.Add("param1", "value1");
        collection.Add("param2", "value2");

        // Assert
        parameterFactory.Received(1).Invoke("param1", 1);
        parameterFactory.Received(1).Invoke("param2", 2);
    }
}