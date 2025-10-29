using System.Text;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteDbFactory : ISqliteDbFactory
{
    private readonly Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> ddlSqlSynthesizerFactory;
    
    public SqliteDbFactory(Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer> ddlSqlSynthesizerFactory)
    {
        this.ddlSqlSynthesizerFactory = ddlSqlSynthesizerFactory ?? throw new  ArgumentNullException(nameof(ddlSqlSynthesizerFactory));
    }

    public void Create(SqliteDbSchema schema, ISqliteConnection connection)
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));
        if (connection is null) throw new ArgumentNullException(nameof(connection));
        if (!connection.Connected) throw new InvalidOperationException("The database connection is not open.");
        if (IsDatabaseAlreadyInitialized(connection)) throw new InvalidOperationException("The database already created and initialized.");
        var sql = SynthesizeCreateTablesAndIndexes(schema);
        using (var cmd = connection.CreateCommand())
        {
            cmd.ExecuteNonQuery(sql);
        }
    }

    public bool IsDatabaseAlreadyInitialized(ISqliteConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.Parameters.Add("TableName", OrmConstants.OrmMigrationsTableName);
        return
            cmd.ExecuteScalar<int?>(
                "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = :TableName);") ==
            1;
    }

    private string SynthesizeCreateTablesAndIndexes(SqliteDbSchema schema)
    {
        var tableSynthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.TableOps, schema);
        var indexSynthesizer = ddlSqlSynthesizerFactory(SqliteDdlSqlSynthesisKind.IndexOps, schema);
        
        var sb = new StringBuilder();
        sb.AppendLine("SAVEPOINT 'create_db';");

        sb.AppendLine("SAVEPOINT 'create_tables';");
        foreach (var table in schema.Tables.Values)
        {
            sb.AppendLine(tableSynthesizer.SynthesizeCreate(table.Name));
        }
        sb.AppendLine("RELEASE SAVEPOINT 'create_tables';");

        if (schema.Indexes.Count != 0)
        {
            sb.AppendLine("SAVEPOINT 'create_indexes';");
            foreach (var index in schema.Indexes.Values)
            {
                sb.AppendLine(indexSynthesizer.SynthesizeCreate(index.IndexName));
            }

            sb.AppendLine("RELEASE SAVEPOINT 'create_indexes';");
        }

        sb.AppendLine("RELEASE SAVEPOINT 'create_db';");
        
        return sb.ToString();
    }
}