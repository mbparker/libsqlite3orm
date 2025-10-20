using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers.Base;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;

public class SqliteSelectSqlSynthesizer : SqliteDmlSqlSynthesizerBase
{
    private readonly Func<SqliteDbSchema, ISqliteWhereClauseBuilder> whereClauseBuilderFactory;
    
    public SqliteSelectSqlSynthesizer(SqliteDbSchema schema, Func<SqliteDbSchema, ISqliteWhereClauseBuilder> whereClauseBuilderFactory) 
        : base(schema)
    {
        this.whereClauseBuilderFactory = whereClauseBuilderFactory;
    }

    public override DmlSqlSynthesisResult Synthesize(Type entityType, SqliteDmlSqlSynthesisArgs args)
    {
        var entityTypeName = entityType.AssemblyQualifiedName;
        var table = Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);
        if (table is not null)
        {
            IReadOnlyDictionary<string, ExtractedParameter> extractedParams = null;
            HashSet<string> otherTablesReferenced = new(StringComparer.OrdinalIgnoreCase);

            var sb = new StringBuilder();

            var selectArgs = args.GetArgs<SynthesizeSelectSqlArgs>();

            void ReferenceTable(string otherTableName)
            {
                var isValid = table.NavigationProperties.Any(x =>
                    x.Kind == SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToOne &&
                    x.ForeignKeyTableName == table.Name);
                if (isValid)
                    otherTablesReferenced.Add(otherTableName);
            }
            
            if (selectArgs.RecursiveLoad)
            {
                foreach (var detProp in table.NavigationProperties)
                {
                    ReferenceTable(detProp.ReferencedEntityTableName);
                }
            }

            // Compute Filter
            string whereClause = null;
            if (selectArgs.FilterExpr is not null)
            {
                var wcb = whereClauseBuilderFactory(Schema);
                whereClause = wcb.Build(entityType, selectArgs.FilterExpr);
                extractedParams = wcb.ExtractedParameters;
                foreach (var item in wcb.ReferencedTables)
                {
                    ReferenceTable(item);
                }
            }

            // Compute Sort
            var sortFields = new List<string>();
            if (selectArgs.AggFunc is null)
            {
                if (selectArgs.SortSpecs?.Any() ?? false)
                {
                    foreach (var sortSpec in selectArgs.SortSpecs)
                    {
                        var dir = sortSpec.Descending ? "DESC" : "ASC";
                        var fieldName = $"{sortSpec.TableName}.{sortSpec.FieldName}";
                        sortFields.Add($"{fieldName} {dir}");
                        ReferenceTable(sortSpec.TableName);
                    }
                }
            }

            // Field selection
            if (selectArgs.AggFunc is not null)
            {
                string colName;
                switch (selectArgs.AggFunc)
                {
                    case SqliteAggregateFunction.Count:
                        sb.Append($"SELECT COUNT(*) AS AF_COUNT FROM {table.Name}");
                        break;
                    case SqliteAggregateFunction.Sum:
                        colName = table.Columns.Values
                            .Single(x => x.ModelFieldName == selectArgs.AggTargetMember.Name).Name;
                        sb.Append($"SELECT SUM({colName}) AS AF_SUM FROM {table.Name}");
                        break;
                    case SqliteAggregateFunction.Total:
                        colName = table.Columns.Values
                            .Single(x => x.ModelFieldName == selectArgs.AggTargetMember.Name).Name;
                        sb.Append($"SELECT TOTAL({colName}) AS AF_TOTAL FROM {table.Name}");
                        break;
                    case SqliteAggregateFunction.Min:
                        colName = table.Columns.Values
                            .Single(x => x.ModelFieldName == selectArgs.AggTargetMember.Name).Name;
                        sb.Append($"SELECT MIN({colName}) AS AF_MIN FROM {table.Name}");
                        break;
                    case SqliteAggregateFunction.Max:
                        colName = table.Columns.Values
                            .Single(x => x.ModelFieldName == selectArgs.AggTargetMember.Name).Name;
                        sb.Append($"SELECT MAX({colName}) AS AF_MAX FROM {table.Name}");
                        break;
                    case SqliteAggregateFunction.Avg:
                        colName = table.Columns.Values
                            .Single(x => x.ModelFieldName == selectArgs.AggTargetMember.Name).Name;
                        sb.Append($"SELECT AVG({colName}) AS AF_AVG FROM {table.Name}");
                        break;
                    default:
                        throw new InvalidEnumArgumentException(nameof(selectArgs.AggFunc), (int)selectArgs.AggFunc,
                            typeof(SqliteAggregateFunction));
                }
            }
            else
            {
                var cols = table.Columns.Values.OrderBy(x => x.Name)
                    .Select(x => $"{table.Name}.{x.Name} AS {table.Name}{x.Name}").ToList();
                if (otherTablesReferenced.Any())
                {
                    foreach (var otherTable in otherTablesReferenced)
                    {
                        cols.AddRange(Schema.Tables[otherTable].Columns.Values.OrderBy(x => x.Name)
                            .Select(x => $"{otherTable}.{x.Name} AS {otherTable}{x.Name}").ToList());
                    }
                }

                sb.Append($"SELECT {string.Join(", ", cols)} FROM {table.Name}");
            }

            // Join any additional tables
            if (otherTablesReferenced.Any())
            {
                foreach (var otherTable in otherTablesReferenced)
                {
                    var np = table.NavigationProperties.FirstOrDefault(x => x.ReferencedEntityTableName == otherTable);
                    if (np is not null)
                    {
                        var fkTable = Schema.Tables[np.ForeignKeyTableName];
                        var fk = fkTable.ForeignKeys
                            .SingleOrDefault(x => x.Id == np.ForeignKeyId);
                        if (fk is not null)
                        {
                            var joinOnSb = new StringBuilder();
                            for (var i = 0; i < fk.KeyFields.Length; i++)
                            {
                                if (i > 0)
                                    joinOnSb.Append(" AND ");
                                joinOnSb.Append(
                                    $"{fkTable.Name}.{fk.KeyFields[i].TableFieldName} = {otherTable}.{fk.KeyFields[i].ForeignTableFieldName}");
                            }

                            sb.Append($" INNER JOIN {otherTable} ON {joinOnSb}");
                        }
                    }
                }
            }
            
            // Filter
            if (!string.IsNullOrWhiteSpace(whereClause))
                sb.Append($" WHERE {whereClause}");

            if (selectArgs.AggFunc is null)
            {
                // Sort
                if (sortFields.Any())
                {
                    sb.Append(" ORDER BY ");
                    sb.Append(string.Join(", ", sortFields));
                }

                // Take
                if (selectArgs.TakeCount.HasValue)
                    sb.Append($" LIMIT {selectArgs.TakeCount.Value}");

                // Skip
                if (selectArgs.SkipCount.HasValue)
                    sb.Append($" OFFSET {selectArgs.SkipCount.Value}");
            }

            return new DmlSqlSynthesisResult(SqliteDmlSqlSynthesisKind.Select, Schema, table,
                Schema.Tables.Values.Where(x => otherTablesReferenced.Contains(x.Name)).ToArray(), 
                sb.ToString(), extractedParams);
        }

        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
}