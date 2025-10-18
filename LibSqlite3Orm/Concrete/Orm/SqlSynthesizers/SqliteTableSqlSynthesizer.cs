using System.Text;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteTableSqlSynthesizer : SqliteDdlSqlSynthesizerBase
{
    public SqliteTableSqlSynthesizer(SqliteDbSchema schema) 
        : base(schema)
    {
    }

    public override string SynthesizeCreate(string objectNameInSchema, string newObjectName = null)
    {
        var table = Schema.Tables[objectNameInSchema];
        
        var sb = new StringBuilder();
        var newTableName = !string.IsNullOrWhiteSpace(newObjectName) ? newObjectName : table.Name;
        sb.Append($"CREATE TABLE IF NOT EXISTS {newTableName} (");
        var firstCol = true;
        foreach (var column in table.Columns.Values)
        {
            if (!firstCol) sb.Append(", ");

            sb.Append($"{column.Name} {GetColumnTypeString(column.DbFieldTypeAffinity)}");

            if (column.Collation.HasValue)
                sb.Append($" COLLATE {GetCollationString(column.Collation.Value)}");

            if (column.IsUnique)
            {
                sb.Append(" UNIQUE");
                if (column.IsUniqueConflictAction.HasValue)
                    sb.Append($" ON CONFLICT {GetConstraintString(column.IsUniqueConflictAction.Value)}");
            }

            if (column.IsNotNull)
            {
                sb.Append(" NOT NULL");
                if (column.IsNotNullConflictAction.HasValue)
                    sb.Append($" ON CONFLICT {GetConstraintString(column.IsNotNullConflictAction.Value)}");
            }

            if (table.PrimaryKey is not null && string.Equals(column.Name, table.PrimaryKey.FieldName))
            {
                var sort = table.PrimaryKey.Ascending ? "ASC" : "DESC";
                sb.Append($" PRIMARY KEY {sort}");
                if (table.PrimaryKey.PrimaryKeyConflictAction.HasValue)
                    sb.Append($" ON CONFLICT {GetConstraintString(table.PrimaryKey.PrimaryKeyConflictAction.Value)}");
                if (table.PrimaryKey.AutoIncrement)
                    sb.Append(" AUTOINCREMENT");
            }

            if (column.DefaultValueLiteral is not null)
                sb.Append($" DEFAULT {column.DefaultValueLiteral}");

            firstCol = false;
        }

        if (table.PrimaryKey is null && table.CompositePrimaryKeyFields.Any())
        {
            sb.Append($", PRIMARY KEY ({string.Join(',', table.CompositePrimaryKeyFields)})");
        }

        if (table.ForeignKeys.Any())
        {
            foreach (var fk in table.ForeignKeys)
            {
                sb.Append(
                    $", FOREIGN KEY ({string.Join(',', fk.KeyFields.Select(x => x.TableFieldName))}) " +
                    $"REFERENCES {fk.ForeignTableName} ({string.Join(',', fk.KeyFields.Select(x => x.ForeignTableFieldName))})");
                if (fk.UpdateAction.HasValue)
                    sb.Append($" ON UPDATE {GetForeignKeyActionString(fk.UpdateAction.Value)}");
                if (fk.DeleteAction.HasValue)
                    sb.Append($" ON DELETE {GetForeignKeyActionString(fk.DeleteAction.Value)}");                
            }
        }

        sb.Append(");");
        return sb.ToString();
    }
    
    public override string SynthesizeDrop(string objectName)
    {
        return $"DROP TABLE IF EXISTS {objectName};";
    }    
}