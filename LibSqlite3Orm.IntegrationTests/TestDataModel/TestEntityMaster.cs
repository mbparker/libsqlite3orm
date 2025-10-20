using System.ComponentModel.DataAnnotations.Schema;
using LibSqlite3Orm.Abstract.Orm;

namespace LibSqlite3Orm.IntegrationTests.TestDataModel;

public class TestEntityMaster
{
    public long Id { get; set; }
    public TestEntityKind EnumValue { get; set; }
    public string StringValue { get; set; }
    public bool BoolValue { get; set; }
    public byte ByteValue { get; set; }
    public sbyte SByteValue { get; set; }
    public ushort UShortValue { get; set; }
    public short ShortValue { get; set; }
    public double? DoubleValue { get; set; }
    public float SingleValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public int IntValue { get; set; }
    public uint UIntValue { get; set; }
    public long LongValue { get; set; }
    public ulong? ULongValue { get; set; }
    public Int128 Int128Value { get; set; }
    public UInt128 UInt128Value { get; set; }
    public Guid? GuidValue { get; set; }
    public DateOnly? DateOnlyValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public TimeOnly? TimeOnlyValue { get; set; }
    public TimeSpan? TimeSpanValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public byte[] BlobValue { get; set; }
    /// <summary>
    /// NotMapped attribute is recommended for clarity. It is deliberately left off here to ensure the logic
    /// for ignoring Lazy types works as expected.
    /// </summary>
    public Lazy<ISqliteQueryable<TestEntityTagLink>> Tags { get; set; }
}