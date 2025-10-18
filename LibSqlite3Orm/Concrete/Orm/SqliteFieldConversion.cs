using LibSqlite3Orm.Abstract.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteFieldConversion : ISqliteFieldConversion
{
    private readonly List<ISqliteFieldConverter> fieldConverters;
    private readonly ISqliteFailoverFieldConverter failoverFieldConverter;
    
    public SqliteFieldConversion(IEnumerable<ISqliteFieldConverter> fieldConverters, ISqliteFailoverFieldConverter failoverFieldConverter)
    {
        this.fieldConverters = fieldConverters.ToList();
        this.failoverFieldConverter = failoverFieldConverter;
    }

    public bool IsConverterRegistered(ISqliteFieldConverter converter)
    {
        return converter is ISqliteFailoverFieldConverter ||
               fieldConverters.Any(x => x.GetType() == converter.GetType());
    }
    
    public void RegisterConverters(params ISqliteFieldConverter[] converters)
    {
        foreach (var converter in converters)
            if (IsConverterRegistered(converter))
                throw new ArgumentException($"Converter {converter.GetType().Name} already registered.");
        fieldConverters.AddRange(converters);
    }

    public bool CanConvert<TFrom, TTo>()
    {
        return CanConvert(typeof(TFrom), typeof(TTo));
    }

    public bool CanConvert(Type typeFrom, Type typeTo)
    {
        return fieldConverters.Any(fc => fc.CanConvert(typeFrom, typeTo)) ||
               failoverFieldConverter.CanConvert(typeFrom, typeTo);
    }

    public TTo Convert<TFrom, TTo>(TFrom value, IFormatProvider formatProvider = null)
    {
        return (TTo)Convert(typeof(TFrom), value, typeof(TTo), formatProvider);
    }

    public object Convert(Type typeFrom, object value, Type typeTo, IFormatProvider formatProvider = null)
    {
        var fc = fieldConverters.FirstOrDefault(x => x.CanConvert(typeFrom, typeTo));
        if (failoverFieldConverter.CanConvert(typeFrom, typeTo)) fc = failoverFieldConverter;
        if (fc is null) throw new InvalidOperationException("None of the registered field converters can make this conversion.");
        return fc.Convert(typeFrom, value, typeTo, formatProvider);
    }
}