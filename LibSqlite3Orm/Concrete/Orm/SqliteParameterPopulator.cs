using System.ComponentModel;
using System.Runtime.Serialization;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;
using LibSqlite3Orm.Types.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteParameterPopulator : ISqliteParameterPopulator
{
    private readonly ISqliteUniqueIdGenerator uniqueIdGenerator;
    private readonly ISqliteFieldValueSerialization serialization;

    public SqliteParameterPopulator(ISqliteUniqueIdGenerator uniqueIdGenerator,
        ISqliteFieldValueSerialization serialization)
    {
        this.uniqueIdGenerator = uniqueIdGenerator;
        this.serialization = serialization;
    }

    public void Populate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection, T entity)
    {
        switch (synthesisResult.SynthesisKind)
        {
            case SqliteDmlSqlSynthesisKind.Insert:
                PopulateForInsert(synthesisResult, parameterCollection, entity);
                break;
            case SqliteDmlSqlSynthesisKind.Update:
                PopulateForUpdate(synthesisResult, parameterCollection, entity);
                break;
            case SqliteDmlSqlSynthesisKind.Select:
            case SqliteDmlSqlSynthesisKind.Delete:
                PopulateForSelectOrDelete(synthesisResult, parameterCollection);
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(synthesisResult.SynthesisKind),
                    (int)synthesisResult.SynthesisKind,
                    typeof(SqliteDmlSqlSynthesisKind));
        }
    }

    public void Populate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection)
    {
        if (synthesisResult.SynthesisKind is SqliteDmlSqlSynthesisKind.Insert or SqliteDmlSqlSynthesisKind.Update)
            throw new InvalidOperationException(
                $"Insert and Delete operations must use the overload that accepts the entity as a parameter.");
        Populate<T>(synthesisResult, parameterCollection, default);
    }

    private void PopulateForSelectOrDelete(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection)
    {
        if (synthesisResult.ExtractedParameters.Count > 0)
        {
            foreach (var parm in synthesisResult.ExtractedParameters.Values)
            {
                parameterCollection.Add(parm.Name, parm.Value);
            }
        }
    }

    private void PopulateForInsert<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection, T entity)
    {
        var type = typeof(T);
        
        if (synthesisResult.Table.PrimaryKey?.AutoGuid ?? false)
        {
            var member = type
                .GetMember(synthesisResult.Table.Columns[synthesisResult.Table.PrimaryKey.FieldName].ModelFieldName)
                .SingleOrDefault();
            member?.SetValue(entity, uniqueIdGenerator.NewUniqueId());
        }
        
        var skipColName = synthesisResult.Table.PrimaryKey?.AutoIncrement ?? false
            ? synthesisResult.Table.PrimaryKey.FieldName
            : null;

        var cols = synthesisResult.Table.Columns.Values.Where(x => !string.Equals(x.Name, skipColName)).OrderBy(x => x.Name).ToArray();
        foreach (var col in cols)
        {
            var member = type.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                parameterCollection.Add(col.Name, member.GetValue(entity));
            }
            else
                throw new InvalidDataContractException(
                    $"Member {col.ModelFieldName} not found on type {type.AssemblyQualifiedName}.");
        }
    }
    
    private void PopulateForUpdate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection, T entity)
    {
        var type = typeof(T);
        
        var keyFieldNames = synthesisResult.Table.CompositePrimaryKeyFields ?? [];
        if (!keyFieldNames.Any())
            keyFieldNames = [synthesisResult.Table.PrimaryKey.FieldName];
        
        var cols = synthesisResult.Table.Columns.Values.Where(x => !keyFieldNames.Contains(x.Name) && !x.IsImmutable).OrderBy(x => x.Name).ToArray();
        foreach (var col in cols)
        {
            var member = type.GetMember(col.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                parameterCollection.Add(col.Name, member.GetValue(entity));
            }
            else
                throw new InvalidDataContractException(
                    $"Member {col.ModelFieldName} not found on type {type.AssemblyQualifiedName}.");
        }
        
        var keyCols = synthesisResult.Table.Columns.Values.Where(x => keyFieldNames.Contains(x.Name)).OrderBy(x => x.Name).ToArray();
        foreach (var keyCol in keyCols)
        {
            var member = type.GetMember(keyCol.ModelFieldName).SingleOrDefault();
            if (member is not null)
            {
                parameterCollection.Add(keyCol.Name, member.GetValue(entity));
            }
            else
                throw new InvalidDataContractException(
                    $"Member {keyCol.ModelFieldName} not found on type {type.AssemblyQualifiedName}.");
        }
    }
}