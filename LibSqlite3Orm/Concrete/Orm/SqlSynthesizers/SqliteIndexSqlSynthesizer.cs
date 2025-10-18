using System.Text;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteIndexSqlSynthesizer : SqliteDdlSqlSynthesizerBase
{
    public SqliteIndexSqlSynthesizer(SqliteDbSchema schema) 
        : base(schema)
    {
    }

    public override string SynthesizeCreate(string objectNameInSchema,  string newObjectName = null)
    {
        var index = Schema.Indexes[objectNameInSchema];
        
        var sb = new StringBuilder();

        var unique = index.IsUnique ? " UNIQUE" : string.Empty;

        var newIndexName = !string.IsNullOrWhiteSpace(newObjectName) ? newObjectName : index.IndexName;
        sb.Append($"CREATE{unique} INDEX IF NOT EXISTS {newIndexName} ON {index.TableName} (");
        var firstCol = true;
        foreach (var col in index.Columns)
        {
            if (!firstCol) sb.Append(", ");
            
            sb.Append(col.Name);
            if (col.Collation.HasValue)
                sb.Append($" COLLATE {GetCollationString(col.Collation.Value)}");
            sb.Append(col.SortDescending ? " DESC" : " ASC");
            
            firstCol = false;
        }
        sb.Append(");");
        return sb.ToString();
    }

    public override string SynthesizeDrop(string objectName)
    {
        return $"DROP INDEX IF EXISTS {objectName};";
    }
}