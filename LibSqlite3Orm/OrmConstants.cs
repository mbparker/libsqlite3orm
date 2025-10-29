namespace LibSqlite3Orm;

public static class OrmConstants
{
    public const string OrmMigrationsTableName = "_ORM_schema_migrations";
    public const string OrmMigrationsIndexByDateDescName = "_index_ORM_schema_migrations_timestamp_desc";
    public const long CurrentSchemaFormatVersion = 1;
    public const long OldestCompatibleSchemaFormatVersion = 0;
}