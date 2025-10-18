using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteDetailPropertyLoader : ISqliteDetailPropertyLoader
{
    private readonly Lazy<IEntityDetailGetter> entityDetailGetter;

    public SqliteDetailPropertyLoader(Func<ISqliteOrmDatabaseContext, IEntityDetailGetter> entityDetailGetterFactory,
        ISqliteOrmDatabaseContext context)
    {
        entityDetailGetter = new Lazy<IEntityDetailGetter>(() =>  entityDetailGetterFactory(context));
    }

    public void LoadDetailProperties<TEntity>(TEntity entity, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool loadNavigationProps, ISqliteConnection connection) where TEntity : new()
    {
        var entityType = typeof(TEntity);
        var detailGetterType = typeof(IEntityDetailGetter);
        LoadDetailEntityProperty(entity, table, row, loadNavigationProps, connection, detailGetterType, entityType);
        LoadDetailEntityListProperty(entity, table, loadNavigationProps, connection, detailGetterType, entityType);
    }

    private void LoadDetailEntityProperty<TEntity>(TEntity entity, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool loadNavigationProps, ISqliteConnection connection, Type detailGetterType, Type entityType)
        where TEntity : new()
    {
        var getDetailsGeneric = detailGetterType.GetMethod(nameof(IEntityDetailGetter.GetDetails));
        if (getDetailsGeneric is not null)
        {
            foreach (var detailsProp in table.NavigationProperties)
            {
                if (detailsProp.Kind == SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToOne)
                {
                    var member = entityType.GetMember(detailsProp.PropertyEntityMember).SingleOrDefault();
                    if (member is not null)
                    {
                        var detailEntityType = Type.GetType(detailsProp.ReferencedEntityTypeName);
                        if (detailEntityType is not null)
                        {
                            var getDetails =
                                getDetailsGeneric.MakeGenericMethod(entityType, detailEntityType);
                            var detailEntity = getDetails.Invoke(entityDetailGetter.Value,
                                [entity, loadNavigationProps, row, connection]);
                            member.SetValue(entity, detailEntity);
                        }
                    }
                }
            }
        }
    }

    private void LoadDetailEntityListProperty<TEntity>(TEntity entity, SqliteDbSchemaTable table, bool loadNavigationProps,
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
                                [entity, loadNavigationProps, connection]);
                            member.SetValue(entity, queryable);
                        }
                    }
                }
            }
        }
    }
}