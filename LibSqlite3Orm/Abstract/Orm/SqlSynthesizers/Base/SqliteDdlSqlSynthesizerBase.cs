using System.ComponentModel;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;

public abstract class SqliteDdlSqlSynthesizerBase : SqliteSqlSynthesizerBase, ISqliteDdlSqlSynthesizer
{
    protected SqliteDdlSqlSynthesizerBase(SqliteDbSchema schema)
        : base(schema)
    {
    }
    
    public abstract string SynthesizeCreate(string objectNameInSchema, string newObjectName = null);
    
    public abstract string SynthesizeDrop(string objectName);
    
    protected string GetForeignKeyActionString(SqliteForeignKeyAction action)
    {
        switch (action)
        {
            case SqliteForeignKeyAction.NoAction:
                return "NO ACTION";
            case SqliteForeignKeyAction.SetNull:
                return "SET NULL";
            case SqliteForeignKeyAction.Cascade:
                return "CASCADE";
            case SqliteForeignKeyAction.SetDefault:
                return "SET DEFAULT";
            case SqliteForeignKeyAction.Restrict:
                return "RESTRICT";
            default:
                throw new InvalidEnumArgumentException(nameof(action), (int)action,
                    typeof(SqliteForeignKeyAction));
        }
    }    

    protected string GetCollationString(SqliteCollation columnCollation)
    {
        switch (columnCollation)
        {
            case SqliteCollation.Binary:
                return "BINARY";
            case SqliteCollation.AsciiLowercase:
                return "NOCASE";
            case SqliteCollation.RightTrimmed:
                return "RTRIM";
            default:
                throw new InvalidEnumArgumentException(nameof(columnCollation), (int)columnCollation,
                    typeof(SqliteCollation));
        }
    }

    protected string GetConstraintString(SqliteLiteConflictAction constraintAction)
    {
        switch (constraintAction)
        {
            case SqliteLiteConflictAction.Rollback:
                return "ROLLBACK";
            case SqliteLiteConflictAction.Abort:
                return "ABORT";
            case SqliteLiteConflictAction.Fail:
                return "FAIL";
            case SqliteLiteConflictAction.Ignore:
                return "IGNORE";
            case SqliteLiteConflictAction.Replace:
                return "REPLACE";
            default:
                throw new InvalidEnumArgumentException(nameof(constraintAction), (int)constraintAction,
                    typeof(SqliteLiteConflictAction));
        }
    }

    protected string GetColumnTypeString(SqliteDataType typeAffinity)
    {
        switch (typeAffinity)
        {
            case SqliteDataType.Integer:
                return "INTEGER";
            case SqliteDataType.Float:
                return "REAL";
            case SqliteDataType.Text:
                return "TEXT";
            case SqliteDataType.Blob:
                return "BLOB";
            default:
                throw new InvalidEnumArgumentException(nameof(typeAffinity), (int)typeAffinity, typeof(SqliteDataType));
        }
    }    
}