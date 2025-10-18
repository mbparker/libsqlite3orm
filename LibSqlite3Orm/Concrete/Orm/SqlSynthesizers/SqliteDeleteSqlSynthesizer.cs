using System.Runtime.Serialization;
using System.Text;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteDeleteSqlSynthesizer : SqliteDmlSqlSynthesizerBase
{
    private readonly Func<SqliteDbSchema, ISqliteWhereClauseBuilder> whereClauseBuilderFactory;
    
    public SqliteDeleteSqlSynthesizer(SqliteDbSchema schema, Func<SqliteDbSchema, ISqliteWhereClauseBuilder> whereClauseBuilderFactory) 
        : base(schema)
    {
        this.whereClauseBuilderFactory = whereClauseBuilderFactory;
    }

    public override DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args)
    {
        var table = Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityType.AssemblyQualifiedName);
        if (table is not null)
        {
            var sb = new StringBuilder();
            IReadOnlyDictionary<string, ExtractedParameter> extractedParams = null;
            
            sb.Append($"DELETE FROM {table.Name}");
            
            // Filter
            var deleteArgs = args.GetArgs<SynthesizeDeleteSqlArgs>();
            if (deleteArgs.FilterExpr is not null)
            {
                var wcb = whereClauseBuilderFactory(Schema);
                var wc = wcb.Build(entityType, deleteArgs.FilterExpr);
                sb.Append($" WHERE {wc}");
                extractedParams = wcb.ExtractedParameters;
            }
            
            sb.Append(';');
            
            return new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Delete, Schema, table, sb.ToString(), extractedParams);
        }

        throw new InvalidDataContractException($"Type {entityType.AssemblyQualifiedName} is not mapped in the schema.");
    }
}