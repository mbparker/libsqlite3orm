namespace LibSqlite3Orm.Abstract.Orm;

// Don't inherit from IOrderedEnumerable<T> or IOrderedEnumerable because of naming conflicts.
public interface ISqliteEnumerable<T>
{
    T SingleRecord();
    T[] AllRecords();
    IEnumerable<T> AsEnumerable();
    ISqliteEnumerable<T> Skip(int count);
    ISqliteEnumerable<T> Take(int count);
}