using System.Data;
using System.Linq.Expressions;

namespace LibSqlite3Orm.Models.Orm;

public class SqliteSortSpec
{
    internal SqliteSortSpec(SqliteDbSchema schema, Expression keySelectorExpr, bool descending)
    {
        if (keySelectorExpr is LambdaExpression { Body: MemberExpression me })
        {
            var tableClass = me.Member.DeclaringType?.AssemblyQualifiedName;
            if (!string.IsNullOrEmpty(tableClass))
            {
                var table = schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == tableClass);
                if (table is not null)
                {
                    TableName = table.Name;
                    FieldName = table.Columns.Values.SingleOrDefault(x => x.ModelFieldName == me.Member.Name)?.Name;
                    Descending = descending;
                    return;
                }
            }

            throw new InvalidExpressionException("Unable to find table class for " + me.Member.Name);
        }

        throw new InvalidExpressionException(
            $"OrderBy, OrderByDescending, ThenBy, and ThenByDescending predicates must be of type {nameof(LambdaExpression)} with a body of {nameof(MemberExpression)}.");
    }

    public string TableName { get; }
    public string FieldName { get; }
    public bool Descending { get; }
}