using Autofac;
using Autofac.Core;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.OData;
using LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;
using LibSqlite3Orm.Concrete;
using LibSqlite3Orm.Concrete.Orm;
using LibSqlite3Orm.Concrete.Orm.EntityServices;
using LibSqlite3Orm.Concrete.Orm.OData;
using LibSqlite3Orm.Concrete.Orm.SqlSynthesizers;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;
using LibSqlite3Orm.Types.Orm.FieldConverters;
using LibSqlite3Orm.Types.FieldSerializers;

namespace LibSqlite3Orm;

public class ContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<SqliteConnection>().As<ISqliteConnection>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteTransaction>().As<ISqliteTransaction>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteCommand>().As<ISqliteCommand>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteParameter>().As<ISqliteParameter>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteFieldValueSerialization>().As<ISqliteFieldValueSerialization>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteParameterCollection>().As<ISqliteParameterCollection>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteParameterPopulator>().As<ISqliteParameterPopulator>().SingleInstance();
        builder.RegisterType<SqliteEntityPostInsertPrimaryKeySetter>().As<ISqliteEntityPostInsertPrimaryKeySetter>().SingleInstance();
        
        builder.RegisterType<OrmGenerativeLogicTracer>().As<IOrmGenerativeLogicTracer>().SingleInstance();
        
        builder.RegisterType<SqliteDataRow>().As<ISqliteDataRow>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteDataColumn>().As<ISqliteDataColumn>().InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteDataReader>().As<ISqliteDataReader>().InstancePerDependency().ExternallyOwned();

        builder.RegisterType<DateOnlyTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<DateTimeOffsetTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<DateTimeTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<DecimalTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<GuidTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<TimeOnlyTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<TimeSpanTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<BooleanLongFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<CharTextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<EnumLongFieldSerializer>().As<ISqliteEnumFieldSerializer>().InstancePerDependency(); // NOT single instance!
        builder.RegisterType<UInt64TextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<Int128TextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();
        builder.RegisterType<UInt128TextFieldSerializer>().As<ISqliteFieldSerializer>().SingleInstance();

        builder.RegisterType<SqliteFieldConversion>().As<ISqliteFieldConversion>().SingleInstance();
        builder.RegisterType<IntegralFieldConverter>().As<ISqliteFailoverFieldConverter>().SingleInstance();

        builder.RegisterType<EntityCreator>().As<IEntityCreator>().InstancePerDependency();
        builder.RegisterType<EntityUpdater>().As<IEntityUpdater>().InstancePerDependency();
        builder.RegisterType<EntityUpserter>().As<IEntityUpserter>().InstancePerDependency();
        builder.RegisterType<EntityGetter>().As<IEntityGetter>().InstancePerDependency();
        builder.RegisterType<EntityDetailGetter>().As<IEntityDetailGetter>().InstancePerDependency();
        builder.RegisterType<EntityDeleter>().As<IEntityDeleter>().InstancePerDependency();
        builder.RegisterType<EntityServices>().As<IEntityServices>().InstancePerDependency();
        builder.RegisterType<SqliteEntityWriter>().As<ISqliteEntityWriter>().InstancePerDependency(); // Can't be a singleton anymore
        builder.RegisterType<SqliteDetailPropertyLoader>().As<ISqliteDetailPropertyLoader>().InstancePerDependency();
        builder.RegisterType<ODataQueryHandler>().As<IODataQueryHandler>().InstancePerDependency();
        builder.RegisterType<EntityDetailCache>().As<IEntityDetailCache>().InstancePerDependency();
        builder.RegisterType<EntityDetailCacheProvider>().As<IEntityDetailCacheProvider>().SingleInstance();

        builder.RegisterType<SqliteDbSchemaBuilder>();
        builder.RegisterGeneric(typeof(SqliteObjectRelationalMapperDatabaseManager<>)).As(typeof(ISqliteObjectRelationalMapperDatabaseManager<>))
            .InstancePerDependency().ExternallyOwned();
        builder.RegisterGeneric(typeof(SqliteObjectRelationalMapper<>)).As(typeof(ISqliteObjectRelationalMapper<>))
            .InstancePerDependency().ExternallyOwned();
        builder.RegisterType<SqliteOrmSchemaContext>().SingleInstance();
        builder.RegisterType<SqliteFileOperations>().As<ISqliteFileOperations>().SingleInstance();
        builder.RegisterType<SqliteUniqueIdGenerator>().As<ISqliteUniqueIdGenerator>().SingleInstance();
        builder.RegisterType<SqliteDbFactory>().As<ISqliteDbFactory>().SingleInstance();

        builder.RegisterType<SqliteWhereClauseBuilder>().As<ISqliteWhereClauseBuilder>().InstancePerDependency();

        builder.RegisterGeneric(typeof(SqliteDbSchemaMigrator<>)).As(typeof(ISqliteDbSchemaMigrator<>))
            .InstancePerDependency().ExternallyOwned();
        
        builder.RegisterType<SqliteTableSqlSynthesizer>()
            .Keyed<ISqliteDdlSqlSynthesizer>(SqliteDdlSqlSynthesisKind.TableOps).InstancePerDependency();
        builder.RegisterType<SqliteIndexSqlSynthesizer>()
            .Keyed<ISqliteDdlSqlSynthesizer>(SqliteDdlSqlSynthesisKind.IndexOps).InstancePerDependency();
        builder.RegisterType<SqliteInsertSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Insert).InstancePerDependency();
        builder.RegisterType<SqliteUpdateSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Update).InstancePerDependency();
        builder.RegisterType<SqliteDeleteSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Delete).InstancePerDependency();
        builder.RegisterType<SqliteSelectSqlSynthesizer>()
            .Keyed<ISqliteDmlSqlSynthesizer>(SqliteDmlSqlSynthesisKind.Select).InstancePerDependency();   
        
        builder.Register<Func<SqliteDdlSqlSynthesisKind, SqliteDbSchema, ISqliteDdlSqlSynthesizer>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return (kind, schema) =>
            {
                var service = new KeyedService(kind, typeof(ISqliteDdlSqlSynthesizer));
                if (context.TryResolveService(service, [new TypedParameter(typeof(SqliteDbSchema), schema)], out object implementation))
                {
                    return implementation as ISqliteDdlSqlSynthesizer;
                }

                return null;
            };
        });
        
        builder.Register<Func<SqliteDmlSqlSynthesisKind, SqliteDbSchema, ISqliteDmlSqlSynthesizer>>(c =>
        {
            var context = c.Resolve<IComponentContext>();
            return (kind, schema) =>
            {
                var service = new KeyedService(kind, typeof(ISqliteDmlSqlSynthesizer));
                if (context.TryResolveService(service, [new TypedParameter(typeof(SqliteDbSchema), schema)], out object implementation))
                {
                    return implementation as ISqliteDmlSqlSynthesizer;
                }

                return null;
            };
        });        
    }
}