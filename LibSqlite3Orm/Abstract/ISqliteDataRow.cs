namespace LibSqlite3Orm.Abstract;

public interface ISqliteDataRow : IEnumerable<ISqliteDataColumn>
{
    int ColumnCount { get; }
    ISqliteDataColumn this[int index] { get; }
    ISqliteDataColumn this[string name] { get; }
}