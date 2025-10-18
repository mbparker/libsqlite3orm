using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteEntityPostInsertPrimaryKeySetter : ISqliteEntityPostInsertPrimaryKeySetter
{
    public void SetAutoIncrementedPrimaryKeyOnEntityIfNeeded<T>(SqliteDbSchema schema, ISqliteConnection connection, T entity)
    {
        var type = typeof(T);
        var table = schema.Tables.Values.SingleOrDefault(x => x.ModelTypeName == type.AssemblyQualifiedName);
        if (table is not null)
        {
            var autoIncFieldName = table.PrimaryKey?.AutoIncrement ?? false ? table.PrimaryKey.FieldName : null;
            if (!string.IsNullOrWhiteSpace(autoIncFieldName))
            {
                var id = connection.GetLastInsertedId();
                var col = table.Columns[autoIncFieldName];
                var member = type.GetMember(col.ModelFieldName).Single();
                member.SetValue(entity, id);
            }
        }
        else
            throw new InvalidDataContractException($"Type {type.AssemblyQualifiedName} is not mapped in the schema.");
    }
}