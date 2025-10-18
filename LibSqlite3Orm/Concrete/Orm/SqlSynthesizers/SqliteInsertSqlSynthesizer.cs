using System.Runtime.Serialization;
using System.Text;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteInsertSqlSynthesizer : SqliteDmlSqlSynthesizerBase
{
    public SqliteInsertSqlSynthesizer(SqliteDbSchema schema) 
        : base(schema)
    {
    }

    public override DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args)
    {
        var table = Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityType.AssemblyQualifiedName);
        if (table is not null)
        {
            var skipColName = table.PrimaryKey?.AutoIncrement ?? false ? table.PrimaryKey.FieldName : null;
            var cols = table.Columns.Values.Where(x => !string.Equals(x.Name, skipColName)).OrderBy(x => x.Name).ToArray();
            var colNames = cols.Select(x => x.Name).ToArray();
            var paramNames = colNames.Select(x => $":{x}").ToArray();
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {table.Name} (");
            sb.Append(string.Join(", ", colNames));
            sb.Append(") VALUES (");
            sb.Append(string.Join(", ", paramNames));
            sb.Append(");");
            return new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Insert, Schema, table, sb.ToString(), null);
        }

        throw new InvalidDataContractException($"Type {entityType.AssemblyQualifiedName} is not mapped in the schema.");
    }
}