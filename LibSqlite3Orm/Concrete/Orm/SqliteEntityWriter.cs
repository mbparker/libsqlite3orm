using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteEntityWriter : ISqliteEntityWriter
{
    private readonly ISqliteDetailPropertyLoader detailPropertyLoader;

    public SqliteEntityWriter(Func<ISqliteOrmDatabaseContext, ISqliteDetailPropertyLoader> detailPropertyLoaderFactory,
        ISqliteOrmDatabaseContext context)
    {
        detailPropertyLoader = detailPropertyLoaderFactory(context);
    }

    public TEntity Deserialize<TEntity>(SqliteDbSchema schema, SqliteDbSchemaTable table, ISqliteDataRow row) where TEntity : new()
    {
        return Deserialize<TEntity>(schema, table, row, false, null);
    }

    public TEntity Deserialize<TEntity>(SqliteDbSchema schema, SqliteDbSchemaTable table, ISqliteDataRow row,
        bool recursiveLoad, ISqliteConnection connection) where TEntity : new()
    {
        var entity = new TEntity();
        var entityType = entity.GetType();
        var cols = table.Columns
            .OrderBy(x => x.Key)
            .Select(x => x.Value)
            .ToArray();

        foreach (var col in cols)
        {
            var member = entityType.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                var rowField = row[table.Name + col.Name];
                member.SetValue(entity, rowField.ValueAs(Type.GetType(col.ModelFieldTypeName)));
            }
        }

        detailPropertyLoader.LoadDetailProperties(entity, table, row, recursiveLoad, connection);

        return entity;
    }
}