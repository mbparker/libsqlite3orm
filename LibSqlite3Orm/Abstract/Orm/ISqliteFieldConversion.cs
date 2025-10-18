namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteFieldConversion
{
    bool IsConverterRegistered(ISqliteFieldConverter converter);
    void RegisterConverters(params ISqliteFieldConverter[] converters);
    bool CanConvert<TFrom, TTo>();
    bool CanConvert(Type typeFrom, Type typeTo);
    TTo Convert<TFrom, TTo>(TFrom value, IFormatProvider formatProvider = null);
    object Convert(Type typeFrom, object value, Type typeTo, IFormatProvider formatProvider = null);
}