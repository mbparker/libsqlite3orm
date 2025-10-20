using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm.EntityServices;

public class EntityDetailGetter : IEntityDetailGetter
{
    private readonly ISqliteOrmDatabaseContext context;
    private readonly Lazy<IEntityGetter> entityGetter;
    private readonly Lazy<ISqliteDetailPropertyLoader> detailPropertyLoader;

    public EntityDetailGetter(Func<ISqliteOrmDatabaseContext, IEntityGetter> entityGetterFactory,
        Func<ISqliteOrmDatabaseContext, ISqliteDetailPropertyLoader> detailPropertyLoaderFactory, 
        ISqliteOrmDatabaseContext context)
    {
        this.context = context;
        // This Lazy load breaks the circular dependency that exists via SqlEntityWriter.
        entityGetter = new Lazy<IEntityGetter>(() => entityGetterFactory(this.context));
        detailPropertyLoader = new  Lazy<ISqliteDetailPropertyLoader>(() => detailPropertyLoaderFactory(this.context));
    }

    public Lazy<TDetails> GetDetails<TTable, TDetails>(TTable record, bool recursiveLoad,
        ISqliteDataRow row, ISqliteConnection connection) where TDetails : new()
    {
        return GetDetailsFromRow<TDetails>(recursiveLoad, row, connection) ??
               GetDetailsFromNewQuery<TTable, TDetails>(record, recursiveLoad, connection);
    }
    
    public Lazy<ISqliteQueryable<TDetailEntity>> GetDetailsList<TEntity, TDetailEntity>(TEntity record, bool recursiveLoad, ISqliteConnection connection) where TDetailEntity : new()
    {
        if (!recursiveLoad || connection is null)
        {
            return new Lazy<ISqliteQueryable<TDetailEntity>>(default(ISqliteQueryable<TDetailEntity>));
        }
        
        return new Lazy<ISqliteQueryable<TDetailEntity>>(() => GetDetailsQueryable<TEntity, TDetailEntity>(record, connection));
    }
    
    private Lazy<TEntity> GetDetailsFromRow<TEntity>(bool recursiveLoad, ISqliteDataRow row, ISqliteConnection connection) where TEntity : new()
    {
        if (!recursiveLoad || connection is null)
        {
            return new Lazy<TEntity>(default(TEntity));
        }
        
        var entityType = typeof(TEntity);
        var entityTypeName = entityType.AssemblyQualifiedName;
        var table = context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityTypeName);

        if (table is not null)
        {
            var cols = table
                .Columns
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToArray();
            if (cols.Any(x => row[table.Name + x.Name] is null))
                return null;
            return new Lazy<TEntity>(() =>
            {
                var entity =
                    (TEntity)entityType
                        .GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, [])
                        ?.Invoke(null) ??
                    throw new ApplicationException(
                        $"Cannot locate constructor on type {entityType.AssemblyQualifiedName}");
                foreach (var col in cols)
                {
                    var member = entityType.GetMember(col.ModelFieldName).SingleOrDefault();
                    if (member is not null)
                    {
                        var rowField = row[table.Name + col.Name];
                        member.SetValue(entity, rowField.ValueAs(Type.GetType(col.ModelFieldTypeName)));
                    }
                }

                detailPropertyLoader.Value.LoadDetailProperties<TEntity>(entity, table, row, recursiveLoad, connection);

                return (TEntity)entity;
            });
        }

        throw new InvalidDataContractException($"Type {entityTypeName} is not mapped in the schema.");
    }
    
    private Lazy<TDetails> GetDetailsFromNewQuery<TTable, TDetails>(TTable record, bool recursiveLoad, ISqliteConnection connection) where TDetails : new()
    {
        if (!recursiveLoad || connection is null)
        {
            return new Lazy<TDetails>(default(TDetails));
        }

        return new Lazy<TDetails>(() =>
        {
            var enumerable = GetDetailsQueryable<TTable, TDetails>(record, connection);
            return enumerable.SingleRecord();
        });
    }

    private ISqliteQueryable<TDetailEntity> GetDetailsQueryable<TEntity, TDetailEntity>(TEntity record,
        ISqliteConnection connection) where TDetailEntity : new()
    {
        // Initialize the queryable recursively, then build an expression in code to filter the results to the current record.
        var queryable = entityGetter.Value.Get<TDetailEntity>(connection, recursiveLoad: true);
        var entityType = typeof(TEntity);
        var entityTypeName = entityType.AssemblyQualifiedName;
        var detailEntityType = typeof(TDetailEntity);
        var detailEntityTypeName = detailEntityType.AssemblyQualifiedName;
        var entityTable =
            context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == entityType.AssemblyQualifiedName);
        var detailEntityTable =
            context.Schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == detailEntityTypeName);
        if (entityTable is not null && detailEntityTable is not null)
        {
            var whereMethod = queryable.GetType().GetMethod(nameof(ISqliteQueryable<TDetailEntity>.Where));
            if (whereMethod is not null)
            {
                var navProp = entityTable.NavigationProperties.SingleOrDefault(x =>
                    x.Kind == SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToMany &&
                    x.PropertyEntityTypeName == entityTypeName &&
                    x.ReferencedEntityTypeName == detailEntityTypeName);

                if (navProp is not null)
                {
                    var fk = context.Schema.Tables[navProp.ForeignKeyTableName].ForeignKeys
                        .Single(x => x.Id == navProp.ForeignKeyId);
                    var fkFromEntityTable = fk.ForeignTableName == detailEntityTable.Name;

                    if (fk is not null)
                    {
                        Expression<Func<TDetailEntity, bool>> wherePredicate = null;
                        for (var i = 0; i < fk.KeyFields.Length; i++)
                        {
                            // This is from the perspective of the details table, since were getting our info from the foreign key specs.
                            // Examples in plain text:
                            // x => x.ForeignId == 1234
                            // x => x.ForeignId1 == 1234 && x.ForeignId2 == 5678 
                            //
                            // Get the member info for both sides. On the master table, we will use it to get a live value.
                            var rightSideOperandMember = entityType
                                .GetMember(fkFromEntityTable
                                    ? fk.KeyFields[i].TableModelProperty
                                    : fk.KeyFields[i].ForeignTableModelProperty)
                                .Single();
                            // On the detail side, we are using a member access expression to build a where clause later.
                            var leftSideOperandMember = detailEntityType
                                .GetMember(fkFromEntityTable
                                    ? fk.KeyFields[i].ForeignTableModelProperty
                                    : fk.KeyFields[i].TableModelProperty)
                                .Single();
                            // Define "x" with a param expression.
                            var detailEntityTypeParamExpr = Expression.Parameter(detailEntityType, "x");
                            // Specify what member on "x" we are accessing
                            var leftSideMemberExpr =
                                Expression.MakeMemberAccess(detailEntityTypeParamExpr, leftSideOperandMember);
                            // Create a constant value expression for comparison against
                            var rightSideValueConstExpr = Expression.Constant(rightSideOperandMember.GetValue(record),
                                rightSideOperandMember.GetValueType());
                            // Now build the equals binary expression
                            var equalComparisonExpr = Expression.MakeBinary(ExpressionType.Equal, leftSideMemberExpr,
                                rightSideValueConstExpr);
                            // Lastly, wrap the binary expression in a lambda expression to match the Where method's input predicate type
                            var lambdaWrapperExpr =
                                Expression.Lambda<Func<TDetailEntity, bool>>(equalComparisonExpr,
                                    detailEntityTypeParamExpr);
                            // Set the predicate expression. If we already set one, link them together with a logical AND expression.
                            if (wherePredicate is null)
                                wherePredicate = lambdaWrapperExpr;
                            else
                                wherePredicate =
                                    Expression.Lambda<Func<TDetailEntity, bool>>(Expression.AndAlso(wherePredicate.Body,
                                        lambdaWrapperExpr.Body), detailEntityTypeParamExpr);
                        }

                        // Now that we've built the predicate we can manually invoke the Where function which will build the where clause when the queryable is finally enumerated.
                        return whereMethod.Invoke(queryable, [wherePredicate]) as ISqliteQueryable<TDetailEntity>;
                    }
                }
            }
        }

        throw new InvalidDataContractException($"Type {detailEntityTypeName} is not mapped in the schema.");
    }
}