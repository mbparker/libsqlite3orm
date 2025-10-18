using System.Collections;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.PInvoke;

namespace LibSqlite3Orm.Concrete;

public class SqliteDataRow : ISqliteDataRow
{
    private readonly IntPtr statement;
    private readonly Func<int, string, IntPtr, ISqliteDataColumn> columnFactory;
    private readonly List<ISqliteDataColumn> columns = new();
    private readonly Dictionary<string, ISqliteDataColumn> columnLookup = new(StringComparer.OrdinalIgnoreCase);
    
    public SqliteDataRow(IntPtr statement, Func<int, string, IntPtr, ISqliteDataColumn> columnFactory)
    {
        this.statement = statement;
        this.columnFactory = columnFactory;
        ColumnCount = SqliteExternals.ColumnCount(statement);
        PopulateColumns();
    }

    public int ColumnCount { get; }

    public ISqliteDataColumn this[int index] => columns[index];

    public ISqliteDataColumn this[string name]
    {
        get
        {
            columnLookup.TryGetValue(name, out var result);
            return result;
        }
    }

    public IEnumerator<ISqliteDataColumn> GetEnumerator()
    {
        return columns.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }    
    
    private void PopulateColumns()
    {
        if (ColumnCount == 0) return;
        for (var i = 0; i < ColumnCount; i++)
        {
            var colName = SqliteExternals.ColumnName(statement, i);
            var column = columnFactory.Invoke(i, colName, statement);
            columns.Add(column);
            columnLookup.Add(colName, column);
        }
    }    
}