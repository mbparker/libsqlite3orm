namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteFieldConverter
{
    bool CanConvert(Type typeFrom, Type typeTo);
    object Convert(Type typeFrom, object value, Type typeTo, IFormatProvider formatProvider = null);
}

public interface ISqliteFailoverFieldConverter : ISqliteFieldConverter;