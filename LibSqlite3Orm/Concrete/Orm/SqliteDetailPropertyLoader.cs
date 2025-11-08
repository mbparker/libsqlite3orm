using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteDetailPropertyLoader : ISqliteDetailPropertyLoader
{
    private readonly Lazy<IEntityDetailGetter> entityDetailGetter;
    private readonly IEntityDetailCacheProvider _entityDetailCacheProvider;
    private readonly ISqliteOrmDatabaseContext context;

    public SqliteDetailPropertyLoader(Func<ISqliteOrmDatabaseContext, IEntityDetailGetter> entityDetailGetterFactory,
        IEntityDetailCacheProvider entityDetailCacheProvider, ISqliteOrmDatabaseContext context)
    {
        entityDetailGetter = new Lazy<IEntityDetailGetter>(() =>  entityDetailGetterFactory(context));
        this._entityDetailCacheProvider = entityDetailCacheProvider;
        this.context = context;
    }

    public void LoadDetailProperties<TEntity>(TEntity entity, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool recursiveLoad, ISqliteConnection connection) where TEntity : new()
    {
        var entityType = typeof(TEntity);
        var detailGetterType = typeof(IEntityDetailGetter);
        LoadDetailEntityProperty(entity, table, row, recursiveLoad, connection, detailGetterType, entityType);
        LoadDetailEntityListProperty(entity, table, recursiveLoad, connection, detailGetterType, entityType);
    }

    private void LoadDetailEntityProperty<TEntity>(TEntity entity, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool recursiveLoad, ISqliteConnection connection, Type detailGetterType, Type entityType)
        where TEntity : new()
    {
        var getDetailsGeneric = detailGetterType.GetMethod(nameof(IEntityDetailGetter.GetDetails));
        if (getDetailsGeneric is not null)
        {
            foreach (var detailsProp in table.NavigationProperties)
            {
                if (detailsProp.Kind == SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToOne)
                {
                    var doNotLoad = false;
                    var fk = table.ForeignKeys.Single(x =>
                        x.Id == detailsProp.ForeignKeyId);
                    var detailEntityType = Type.GetType(detailsProp.ReferencedEntityTypeName);
                    if (detailEntityType is null) continue;
                    
                    if (fk.Optional)
                    {
                        for (var i = 0; i < fk.KeyFields.Length; i++)
                        {
                            var col = row[fk.ForeignTableName + fk.KeyFields[i].ForeignTableFieldName];
                            if (col is null) break;
                            if (col.Value() is null)
                            {
                                // Optional FK entity is null for this record. This bool will make the details getter return a Lazy<T>(null)
                                // This is necessary because we don't want a null Lazy<T> - we want its Value property to return the null.
                                doNotLoad = true;
                                break;
                            }
                        }
                    }

                    var member = entityType.GetMember(detailsProp.PropertyEntityMember).SingleOrDefault();
                    if (member is not null)
                    {
                        var entityCache = _entityDetailCacheProvider.GetCache(context, connection);
                        var detailEntity = entityCache.TryGet(entity, detailsProp);
                        if (detailEntity is null)
                        {
                            var getDetails =
                                getDetailsGeneric.MakeGenericMethod(entityType, detailEntityType);
                            detailEntity = getDetails.Invoke(entityDetailGetter.Value,
                                [entity, !doNotLoad && recursiveLoad, row, connection]);
                            entityCache.Upsert(entity, detailEntity, detailsProp);
                        }
                        
                        member.SetValue(entity, detailEntity);
                    }
                }
            }
        }
    }

    private void LoadDetailEntityListProperty<TEntity>(TEntity entity, SqliteDbSchemaTable table, bool recursiveLoad,
        ISqliteConnection connection, Type detailGetterType, Type entityType) where TEntity : new()
    {
        var getDetailsListGeneric = detailGetterType.GetMethod(nameof(IEntityDetailGetter.GetDetailsList));
        if (getDetailsListGeneric is not null)
        {
            foreach (var detailsProp in table.NavigationProperties)
            {
                if (detailsProp.Kind == SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToMany)
                {
                    var member = entityType.GetMember(detailsProp.PropertyEntityMember).SingleOrDefault();
                    if (member is not null)
                    {
                        var detailEntityType = Type.GetType(detailsProp.ReferencedEntityTypeName);
                        if (detailEntityType is not null)
                        {
                            var getDetailsList =
                                getDetailsListGeneric.MakeGenericMethod(entityType, detailEntityType);
                            var queryable = getDetailsList.Invoke(entityDetailGetter.Value,
                                [entity, recursiveLoad, connection]);
                            member.SetValue(entity, queryable);
                        }
                    }
                }
            }
        }
    }
}