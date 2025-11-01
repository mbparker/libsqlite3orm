using System.Data;
using System.Linq.Expressions;

namespace LibSqlite3Orm.Models.Orm;

public class SqliteSortSpec
{
    internal SqliteSortSpec(SqliteDbSchema schema, Type entityModel, Expression keySelectorExpr, bool descending)
    {
        if (keySelectorExpr is LambdaExpression { Body: MemberExpression me })
        {
            var entityModelClass = entityModel.AssemblyQualifiedName;
            
            // Check for a navigation property
            var me2 = me;
            while (me2.Expression is MemberExpression me3)
                me2 = me3;
            var memberType = me2.Member.GetValueType();
            if (memberType.IsLazy())
            {
                memberType = memberType.GetGenericArguments()[0];
                entityModelClass = memberType.AssemblyQualifiedName;
            }

            var table = schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityModelClass);
            if (table is not null)
            {
                var prop = table.Columns.Values.SingleOrDefault(x => x.ModelFieldName == me.Member.Name);
                if (prop is not null)
                {
                    TableName = table.Name;
                    FieldName = prop.Name;
                    Descending = descending;
                    return;
                }
            }

            throw new InvalidExpressionException(
                $"Cannot determine database field name. Field or Property '{me.Member.Name}' does not exist on Type '{entityModelClass}'.");
        }

        throw new InvalidExpressionException(
            $"OrderBy, OrderByDescending, ThenBy, and ThenByDescending predicates must be of type {nameof(LambdaExpression)} with a body of {nameof(MemberExpression)}.");
    }

    public string TableName { get; }
    public string FieldName { get; }
    public bool Descending { get; }
}