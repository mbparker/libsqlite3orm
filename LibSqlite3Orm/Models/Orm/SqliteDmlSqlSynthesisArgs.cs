using System.Runtime.Serialization;

namespace LibSqlite3Orm.Models.Orm;

public class SqliteDmlSqlSynthesisArgs
{
    private readonly object args;
    
    public SqliteDmlSqlSynthesisArgs(object args)
    {
        this.args = args;
    }
    
    public TArgs GetArgs<TArgs>() where TArgs : class
    {
        if (args is null) throw new InvalidDataContractException("No arguments provided.");
        if (args is not TArgs result)
            throw new InvalidCastException($"{nameof(args)} is not of type {typeof(TArgs).AssemblyQualifiedName}");
        return result;
    }

    public static SqliteDmlSqlSynthesisArgs Empty => new (null);
}