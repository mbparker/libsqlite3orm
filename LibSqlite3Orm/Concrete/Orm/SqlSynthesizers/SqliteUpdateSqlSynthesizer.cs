using System.Runtime.Serialization;
using System.Text;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteUpdateSqlSynthesizer : SqliteDmlSqlSynthesizerBase
{
    public SqliteUpdateSqlSynthesizer(SqliteDbSchema schema) 
        : base(schema)
    {
    }

    public override DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args)
    {
        var table = Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityType.AssemblyQualifiedName);
        if (table is not null)
        {
            var keyFieldNames = table.CompositePrimaryKeyFields ?? [];
            if (!keyFieldNames.Any())
                keyFieldNames = [table.PrimaryKey.FieldName];
            var cols = table.Columns.Values.Where(x => !keyFieldNames.Contains(x.Name) && !x.IsImmutable).OrderBy(x => x.Name).ToArray();
            var colNames = cols.Select(x => x.Name).ToArray();
            var nonKeyFields = colNames.Select(x => $"{x} = :{x}").ToArray();
            var keyFields = keyFieldNames.Select(x => $"{x} = :{x}").ToArray();
            var sb = new StringBuilder();
            sb.Append($"UPDATE {table.Name} SET ");
            sb.Append(string.Join(", ", nonKeyFields));
            sb.Append(" WHERE (");
            sb.Append(string.Join(" AND ", keyFields));
            sb.Append(");");
            return new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Update, Schema, table, sb.ToString(), null);
        }

        throw new InvalidDataContractException($"Type {entityType.AssemblyQualifiedName} is not mapped in the schema.");
    }
}