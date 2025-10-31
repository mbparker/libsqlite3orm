using System.Data;
using System.Linq.Expressions;

namespace LibSqlite3Orm.Models.Orm;

public class SqliteSortSpec
{
    internal SqliteSortSpec(SqliteDbSchema schema, Type entityModel, Expression keySelectorExpr, bool descending)
    {
        if (keySelectorExpr is LambdaExpression { Body: MemberExpression me })
        {
            var entityModelClass = me.Member.DeclaringType?.AssemblyQualifiedName ?? entityModel.AssemblyQualifiedName;
            if (!string.IsNullOrEmpty(entityModelClass))
            {
                var memExp = me;
                while (memExp.Expression is MemberExpression me2)
                {
                    memExp = me2;
                }
                
                
                
                var table = schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityModelClass);
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