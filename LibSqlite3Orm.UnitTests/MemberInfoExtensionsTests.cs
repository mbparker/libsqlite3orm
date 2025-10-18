using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm;

namespace LibSqlite3Orm.UnitTests;

[TestFixture]
public class MemberInfoExtensionsTests
{
    private class TestClass
    {
        public string TestProperty { get; set; } = "PropertyValue";
        public int TestField = 42;
    }

    [Test]
    public void GetValueType_WithProperty_ReturnsPropertyType()
    {
        // Arrange
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));

        // Act
        var result = propertyInfo.GetValueType();

        // Assert
        Assert.That(result, Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetValueType_WithField_ReturnsFieldType()
    {
        // Arrange
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.TestField));

        // Act
        var result = fieldInfo.GetValueType();

        // Assert
        Assert.That(result, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void GetValueType_WithInvalidMember_ThrowsException()
    {
        // Arrange
        var methodInfo = typeof(TestClass).GetMethod("ToString");

        // Act & Assert
        Assert.Throws<InvalidDataContractException>(() => methodInfo.GetValueType());
    }

    [Test]
    public void GetValue_WithProperty_ReturnsPropertyValue()
    {
        // Arrange
        var testObj = new TestClass { TestProperty = "TestValue" };
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));

        // Act
        var result = propertyInfo.GetValue(testObj);

        // Assert
        Assert.That(result, Is.EqualTo("TestValue"));
    }

    [Test]
    public void GetValue_WithField_ReturnsFieldValue()
    {
        // Arrange
        var testObj = new TestClass { TestField = 123 };
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.TestField));

        // Act
        var result = fieldInfo.GetValue(testObj);

        // Assert
        Assert.That(result, Is.EqualTo(123));
    }

    [Test]
    public void GetValue_WithInvalidMember_ThrowsException()
    {
        // Arrange
        var testObj = new TestClass();
        var methodInfo = typeof(TestClass).GetMethod("ToString");

        // Act & Assert
        Assert.Throws<InvalidDataContractException>(() => methodInfo.GetValue(testObj));
    }

    [Test]
    public void SetValue_WithProperty_SetsPropertyValue()
    {
        // Arrange
        var testObj = new TestClass();
        var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.TestProperty));

        // Act
        propertyInfo.SetValue(testObj, "NewValue");

        // Assert
        Assert.That(testObj.TestProperty, Is.EqualTo("NewValue"));
    }

    [Test]
    public void SetValue_WithField_SetsFieldValue()
    {
        // Arrange
        var testObj = new TestClass();
        var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.TestField));

        // Act
        fieldInfo.SetValue(testObj, 999);

        // Assert
        Assert.That(testObj.TestField, Is.EqualTo(999));
    }

    [Test]
    public void SetValue_WithInvalidMember_ThrowsException()
    {
        // Arrange
        var testObj = new TestClass();
        var methodInfo = typeof(TestClass).GetMethod("ToString");

        // Act & Assert
        Assert.Throws<InvalidDataContractException>(() => methodInfo.SetValue(testObj, "value"));
    }
}