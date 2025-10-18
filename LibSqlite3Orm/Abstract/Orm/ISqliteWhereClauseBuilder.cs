using System.Linq.Expressions;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteWhereClauseBuilder
{
    IReadOnlyDictionary<string, ExtractedParameter> ExtractedParameters{ get; }
    HashSet<string> ReferencedTables { get; }
    string Build(Type entityType, Expression expression);
}