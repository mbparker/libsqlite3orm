using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Autofac;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.IntegrationTests.TestDataModel;

namespace LibSqlite3Orm.IntegrationTests;

public class IntegrationTestSeededBase<TContext> : IntegrationTestBase<TContext> where TContext : class, ISqliteOrmDatabaseContext
{
    protected Dictionary<long, TestEntityMaster> SeededMasterRecords { get; } = new();
    protected Dictionary<long, TestEntityTag> SeededTagRecords { get; } = new();
    protected Dictionary<string, TestEntityTagLink> SeededLinkRecords { get; } = new(StringComparer.OrdinalIgnoreCase);
    
    public override void SetUp()
    {
        SeededTagRecords.Clear();
        SeededLinkRecords.Clear();
        SeededMasterRecords.Clear();
        base.SetUp();
        DoSeedDatabase();
    }

    protected virtual void SeedDatabase()
    {
        var cnt = Rng.Next(10, 50);
        for (var i = 0; i < cnt; i++)
        {
            var entity = CreateTestEntityMasterWithRandomValues();
            Orm.Insert(entity);
            SeededMasterRecords.Add(entity.Id, entity);
        }
        
        cnt = Rng.Next(10, 25);
        for (var i = 0; i < cnt; i++)
        {
            var entity = CreateTestEntityTagWithRandomValues();
            Orm.Insert(entity);
            SeededTagRecords.Add(entity.Id, entity);
        }
        
        foreach(var entity in SeededMasterRecords.Values)
        {
            cnt = Rng.Next(1, 5);
            var tagIds = new HashSet<long>();
            for (var i = 0; i < cnt; i++)
            {
                var tagId = Rng.Next(1, SeededTagRecords.Count);
                if (tagIds.Add(tagId))
                {
                    var link = new TestEntityTagLink { EntityId = entity.Id, TagId = tagId };
                    Orm.Insert(link);
                    SeededLinkRecords.Add(link.Id, link);
                }
            }

        }
    }
    
    private void DoSeedDatabase()
    {
        Orm.BeginTransaction();
        try
        {
            SeedDatabase();
            Orm.CommitTransaction();
        }
        catch (Exception)
        {
            Orm.RollbackTransaction();
            throw;
        }
    }
}

public class IntegrationTestBase<TContext> where TContext : class, ISqliteOrmDatabaseContext
{
    private IContainer container;
    private ISqliteConnection connection;
    
    protected Random Rng { get; } =  new(Environment.TickCount);
    protected ISqliteObjectRelationalMapper<TContext> Orm { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        connection = Resolve<Func<ISqliteConnection>>()();
        connection.OpenInMemory();
        
        using (var dbManager = Resolve<Func<ISqliteObjectRelationalMapperDatabaseManager<TContext>>>().Invoke())
        {
            dbManager.UseConnection(connection.GetReference());
            dbManager.CreateDatabase();
        }

        Orm = Resolve<Func<ISqliteObjectRelationalMapper<TContext>>>().Invoke();
        Orm.UseConnection(connection.GetReference());
    }

    [TearDown]
    public virtual void TearDown()
    {
        Orm.Dispose();
        connection.Dispose();
    }
    
    [OneTimeSetUp]
    public virtual void SetUpFixture()
    {
        CreateContainer();
    }
    
    [OneTimeTearDown]
    public virtual void TearDownFixture()
    {
        DisposeContainer();
    }
    
    protected TService Resolve<TService>()
    {
        return container.Resolve<TService>();
    }

    protected void CreateContainer()
    {
        if (container is null)
            container = ContainerRegistration.RegisterDependencies();
    }

    protected void DisposeContainer()
    {
        container?.Dispose();
        container = null;
    }

    protected void AssertThatRecordsMatch(object expected, object actual)
    {
        Assert.That(actual, Is.Not.Null);
        
        var typeExpected = expected.GetType();
        var typeActual = actual.GetType();
        Assert.That(typeActual, Is.SameAs(typeExpected));
        var members = typeExpected
            .GetMembers(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public).Where(x =>
                x.MemberType.HasFlag(MemberTypes.Field) || x.MemberType.HasFlag(MemberTypes.Property) &&
                x.GetCustomAttributes(typeof(NotMappedAttribute), true).Length == 0);

        foreach (var member in members)
        {
            var expectedValue = member.GetValue(expected);
            var actualValue = member.GetValue(actual);
            if (member.GetValueType().IsArray)
            {
                var actualArray = (IList)actualValue;
                var expectedArray = (IList)expectedValue;
                Assert.That(actualArray?.Count, Is.EqualTo(expectedArray?.Count), $"{member.Name} array length differs.");
                if (expectedArray is not null && actualArray is not null)
                {
                    for (var i = 0; i < expectedArray.Count; i++)
                    {
                        Assert.That(actualArray[i], Is.EqualTo(expectedArray[i]),
                            $"{member.Name} at index {i} differs. Expected {expectedValue ?? "null"} but was {actualValue ?? "null"}");
                    }
                }
            }
            else
            {
                Assert.That(actualValue, Is.EqualTo(expectedValue),
                    $"{member.Name} differs. Expected {expectedValue ?? "null"} but was {actualValue ?? "null"}");
            }
        }
    }

    protected TestEntityTag CreateTestEntityTagWithRandomValues()
    {
        return new TestEntityTag
        {
            TagValue = GenerateRandomStringWithWords(Rng.Next(1, 10)) + " - " + Guid.NewGuid().ToString("N"), // guarantee uniqueness
            Description = GenerateRandomStringWithWords(Rng.Next(1, 100))
        };
    }

    protected TestEntityMaster CreateTestEntityMasterWithMaxValues()
    {
        return new TestEntityMaster
        {
            BlobValue = [0x42, 0x24],
            StringValue = "This entity has all max values.",
            BoolValue = true,
            ByteValue = Byte.MaxValue,
            ShortValue = short.MaxValue,
            UShortValue = ushort.MaxValue,
            IntValue = int.MaxValue,
            UIntValue = uint.MaxValue,
            DateOnlyValue = DateOnly.MaxValue,
            TimeOnlyValue = TimeOnly.MaxValue,
            DateTimeOffsetValue = DateTimeOffset.MaxValue,
            DecimalValue = decimal.MaxValue,
            DoubleValue = double.MaxValue,
            SingleValue = float.MaxValue,
            DateTimeValue = DateTime.MaxValue,
            GuidValue = Guid.NewGuid(),
            EnumValue = TestEntityKind.Kind2,
            Int128Value = Int128.MaxValue,
            LongValue = long.MaxValue,
            SByteValue = SByte.MaxValue,
            TimeSpanValue = TimeSpan.MaxValue,
            UInt128Value = UInt128.MaxValue,
            ULongValue = UInt64.MaxValue
        };        
    }
    
    protected TestEntityMaster CreateTestEntityMasterWithMinValues()
    {
        return new TestEntityMaster
        {
            BlobValue = [0x24, 0x42],
            StringValue = "This entity has all min values.",
            BoolValue = false,
            ByteValue = Byte.MinValue,
            ShortValue = short.MinValue,
            UShortValue = ushort.MinValue,
            IntValue = int.MinValue,
            UIntValue = uint.MinValue,
            DecimalValue = decimal.MinValue,
            DoubleValue = double.MinValue,
            SingleValue = float.MinValue,
            GuidValue = null,
            EnumValue = TestEntityKind.Kind1,
            Int128Value = Int128.MinValue,
            LongValue = long.MinValue,
            SByteValue = SByte.MinValue,
            UInt128Value = UInt128.MinValue,
            ULongValue = UInt64.MinValue,
            DateTimeValue = DateTime.MinValue,
            DateOnlyValue = DateOnly.MinValue,
            TimeOnlyValue = TimeOnly.MinValue,
            DateTimeOffsetValue = DateTimeOffset.MinValue,
            TimeSpanValue = TimeSpan.MinValue
        };        
    } 
    
    protected TestEntityMaster CreateTestEntityMasterWithRandomValues()
    {
        var result = new TestEntityMaster();
        result.StringValue = GenerateRandomStringWithWords(Rng.Next(1, 50));
        result.BlobValue = new byte[Rng.Next(2, 1024)]; 
        Rng.NextBytes(result.BlobValue);
        result.BoolValue = Environment.TickCount % 2 == 0;
        var tempArray = new byte[1];
        Rng.NextBytes(tempArray);
        result.ByteValue = tempArray[0];
        Rng.NextBytes(tempArray);
        result.SByteValue = unchecked((sbyte)tempArray[0]);
        tempArray = new byte[2];
        Rng.NextBytes(tempArray);
        result.ShortValue = BitConverter.ToInt16(tempArray);
        Rng.NextBytes(tempArray);
        result.UShortValue = BitConverter.ToUInt16(tempArray);
        result.SingleValue = Rng.NextSingle();
        result.DoubleValue = Rng.NextDouble();
        result.IntValue = Rng.Next();
        result.LongValue = Rng.NextInt64();
        tempArray = new byte[4];
        Rng.NextBytes(tempArray);
        result.UIntValue = BitConverter.ToUInt32(tempArray);
        tempArray = new byte[8];
        Rng.NextBytes(tempArray);
        result.ULongValue = BitConverter.ToUInt64(tempArray);
        tempArray = new byte[16];
        Rng.NextBytes(tempArray);
        result.Int128Value = BitConverter.ToInt128(tempArray);
        Rng.NextBytes(tempArray);
        result.UInt128Value = BitConverter.ToUInt128(tempArray);
        Rng.NextBytes(tempArray);
        result.GuidValue = new Guid(tempArray);
        result.DecimalValue = GenerateRandomDecimal();
        result.EnumValue = Environment.TickCount % 2 == 0 ?  TestEntityKind.Kind2 : TestEntityKind.Kind1;
        result.DateTimeValue = GenerateRandomDateTime(DateTime.MinValue, DateTime.Now);
        result.DateOnlyValue = DateOnly.FromDateTime(GenerateRandomDateTime(DateTime.MinValue, DateTime.Now));
        result.TimeOnlyValue = TimeOnly.FromDateTime(GenerateRandomDateTime(DateTime.MinValue, DateTime.Now));
        result.DateTimeOffsetValue = DateTimeOffset.Parse(GenerateRandomDateTime(DateTime.MinValue, DateTime.Now).ToString("O"));
        var dt1 = GenerateRandomDateTime(DateTime.MinValue, DateTime.Now);
        var dt2 = GenerateRandomDateTime(DateTime.MinValue, DateTime.Now);
        if (dt1 > dt2)
            result.TimeSpanValue = dt1 - dt2;
        else
            result.TimeSpanValue = dt2 - dt1;
        
        return result;
    }

    private decimal GenerateRandomDecimal()
    {
        // Define the desired range for your random decimal
        var minValue = 10.0m;
        var maxValue = 1000.0m;

        // Generate a random double between 0.0 and 1.0
        var randomDouble = Rng.NextDouble();

        // Scale and shift the random double to fit the desired decimal range
        return (decimal)(randomDouble * (double)(maxValue - minValue) + (double)minValue);
    }
    
    private DateTime GenerateRandomDateTime(DateTime startDate, DateTime endDate)
    {
        // Ensure the start date is before or equal to the end date
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date cannot be after end date.");
        }

        // Calculate the total number of days between the start and end dates
        var dateRange = endDate - startDate;

        // Get a random number of days within the range
        var randomDays = Rng.Next(dateRange.Days + 1);

        // Get a random time of day (hours, minutes, seconds, milliseconds)
        var randomHours = Rng.Next(0, 24);
        var randomMinutes = Rng.Next(0, 60);
        var randomSeconds = Rng.Next(0, 60);
        var randomMilliseconds = Rng.Next(0, 1000);

        // Add the random days and time to the start date
        var randomDate = startDate
            .AddDays(randomDays)
            .AddHours(randomHours)
            .AddMinutes(randomMinutes)
            .AddSeconds(randomSeconds)
            .AddMilliseconds(randomMilliseconds);

        return randomDate;
    }
    
    private static readonly List<string> WordList = new()
    {
        "apple", "banana", "orange", "grape", "strawberry",
        "cat", "dog", "bird", "fish", "rabbit",
        "happy", "sad", "angry", "calm", "excited",
        "run", "jump", "sleep", "eat", "drink"
    };
    
    private string GenerateRandomStringWithWords(int numberOfWords)
    {
        if (numberOfWords == 0) return null;

        StringBuilder resultBuilder = new StringBuilder();

        for (int i = 0; i < numberOfWords; i++)
        {
            int randomIndex = Rng.Next(0, WordList.Count);
            resultBuilder.Append(WordList[randomIndex]);

            if (i < numberOfWords - 1)
            {
                resultBuilder.Append(' '); // Add space between words
            }
        }

        return resultBuilder.ToString();
    }
}