using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO.Hashing;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Types.Orm;

public class SqliteDbSchemaBuilder
{
    private readonly ISqliteFieldValueSerialization serialization;
    private SqliteDbSchemaOptions schemaOptions;

    public SqliteDbSchemaBuilder(ISqliteFieldValueSerialization serialization)
    {
        this.serialization = serialization;
        schemaOptions = new SqliteDbSchemaOptions();
    }

    public SqliteTableOptionsBuilder<TTable> HasTable<TTable>(string tableName = null) where TTable : class, new()
    {
        var options = new SqliteTableOptions(schemaOptions);
        options.TableType = typeof(TTable);
        options.Name = string.IsNullOrWhiteSpace(tableName) ? options.TableType.Name : tableName.Trim();
        schemaOptions.Tables.Add(options.TableType.AssemblyQualifiedName, options);
        return new SqliteTableOptionsBuilder<TTable>(options, serialization);
    }

    public SqliteIndexOptionsBuilder<TTable> HasIndex<TTable>(string indexName = null) where TTable : class, new()
    {
        var options = new SqliteIndexOptions(schemaOptions);
        options.TableType = typeof(TTable);
        options.IndexName = indexName?.Trim();
        if (!schemaOptions.Indexes.TryGetValue(options.TableType.AssemblyQualifiedName, out var tableIndexes))
        {
            tableIndexes = new List<SqliteIndexOptions>();
            schemaOptions.Indexes.Add(options.TableType.AssemblyQualifiedName, tableIndexes);
        }

        tableIndexes.Add(options);
        return new SqliteIndexOptionsBuilder<TTable>(options);
    }

    public SqliteDbSchema Build()
    {
        var result = new SqliteDbSchema();

        foreach (var table in schemaOptions.Tables.Values)
        {
            var schemaTable = new SqliteDbSchemaTable();
            schemaTable.Name = table.Name;
            schemaTable.ModelTypeName = table.TableType.AssemblyQualifiedName;
            foreach (var column in table.Columns.Values)
            {
                var schemaTableCol = new SqliteDbSchemaTableColumn();
                schemaTableCol.Name = column.Name;
                schemaTableCol.ModelFieldName = column.Member.Name;
                var serializedType = column.Member.GetValueType();
                schemaTableCol.ModelFieldTypeName = serializedType.AssemblyQualifiedName;
                schemaTableCol.SerializedFieldTypeName = schemaTableCol.ModelFieldTypeName;

                serializedType = serialization[serializedType]?.SerializedType ?? serializedType;
                schemaTableCol.SerializedFieldTypeName = serializedType.AssemblyQualifiedName;

                var colAffinity = serializedType.GetSqliteDataType();
                if (colAffinity is null)
                    throw new InvalidOperationException(
                        $"Type {serializedType} is not directly storable. Consider using an {nameof(ISqliteFieldSerializer)} implementation for field {schemaTableCol.Name} on table {schemaTable.Name}.");
                schemaTableCol.DbFieldTypeAffinity = colAffinity.Value;
                schemaTableCol.Collation = column.Collation;
                schemaTableCol.DefaultValueLiteral = column.DefaultValueLiteral;
                schemaTableCol.IsNotNull = column.IsNotNull;
                schemaTableCol.IsNotNullConflictAction = column.IsNotNullConflictAction;
                schemaTableCol.IsUnique = column.IsUnique;
                schemaTableCol.IsUniqueConflictAction = column.IsUniqueConflictAction;
                schemaTableCol.IsImmutable = column.IsImmutable;
                schemaTable.Columns.Add(schemaTableCol.Name, schemaTableCol);

                if (!table.CompositePrimaryKeyProperties.Any() && table.PrimaryKeyColumnOptions is not null &&
                    string.Equals(column.Name, table.PrimaryKeyColumnOptions.Name))
                {
                    var pk = new SqliteDbSchemaTablePrimaryKeyColumn();
                    pk.FieldName = column.Name;
                    pk.Ascending = table.PrimaryKeyColumnOptions.Ascending;
                    pk.AutoIncrement = table.PrimaryKeyColumnOptions.AutoIncrement;
                    pk.AutoGuid = table.PrimaryKeyColumnOptions.AutoGuid;
                    pk.PrimaryKeyConflictAction = table.PrimaryKeyColumnOptions.PrimaryKeyConflictAction;
                    schemaTable.PrimaryKey = pk;
                }
            }

            if (table.CompositePrimaryKeyProperties.Any())
            {
                schemaTable.CompositePrimaryKeyFields =
                    table.CompositePrimaryKeyProperties.Select(x => table.Columns[x.Name].Name).ToArray();
            }

            result.Tables.Add(schemaTable.Name, schemaTable);
        }

        foreach (var table in schemaOptions.Tables.Values)
        {
            var thisTable =
                result.Tables.Values.FirstOrDefault(x => x.ModelTypeName == table.TableType.AssemblyQualifiedName);
            foreach (var fko in table.ForeignKeys)
            {
                var foreignTable = result.Tables.Values.FirstOrDefault(x =>
                    x.ModelTypeName == fko.ForeignTableType.AssemblyQualifiedName);
                if (thisTable is not null && foreignTable is not null)
                {
                    if (thisTable != foreignTable)
                    {
                        var fk = new SqliteDbSchemaTableForeignKey();
                        fk.Id = thisTable.ForeignKeys.Count + 1;
                        fk.KeyFields = fko.ModelProperties.Select(x => new SqliteDbSchemaTableForeignKeyFieldPair
                        {
                            TableModelProperty = x.TableProperty.Name, 
                            TableFieldName = table.Columns[x.TableProperty.Name].Name,
                            ForeignTableModelProperty = x.ForeignTableProperty.Name,
                            ForeignTableFieldName = fko.TableOptions.Columns[x.ForeignTableProperty.Name].Name
                            
                        }).ToArray();
                        fk.ForeignTableName = foreignTable.Name;
                        fk.ForeignTableModelTypeName = foreignTable.ModelTypeName;
                        fk.UpdateAction = fko.UpdateAction;
                        fk.DeleteAction = fko.DeleteAction;
                        thisTable.ForeignKeys.Add(fk);
                        
                        foreach (var np in fko.NavigationProperties.Values)
                        {
                            if (foreignTable.ModelTypeName == np.ReferencedEntityType.AssemblyQualifiedName)
                            {
                                thisTable.NavigationProperties.Add(new SqliteDbSchemaTableForeignKeyNavigationProperty
                                {
                                    ForeignKeyTableName = thisTable.Name,
                                    ForeignKeyId = fk.Id,
                                    Kind = np.Kind == SqliteTableForeignKeyNavigationPropertyKind.OneToOne
                                        ? SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToOne
                                        : SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToMany,
                                    PropertyEntityTypeName = np.PropertyEntityType.AssemblyQualifiedName,
                                    PropertyEntityTableName = thisTable.Name,
                                    PropertyEntityMember = np.PropertyEntityMember.Name,
                                    ReferencedEntityTypeName = foreignTable.ModelTypeName,
                                    ReferencedEntityTableName = foreignTable.Name
                                });
                            }
                            else if (thisTable.ModelTypeName == np.ReferencedEntityType.AssemblyQualifiedName)
                            {
                                foreignTable.NavigationProperties.Add(new SqliteDbSchemaTableForeignKeyNavigationProperty
                                {
                                    ForeignKeyTableName = thisTable.Name,
                                    ForeignKeyId = fk.Id,
                                    Kind = np.Kind == SqliteTableForeignKeyNavigationPropertyKind.OneToOne
                                        ? SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToOne
                                        : SqliteDbSchemaTableForeignKeyNavigationPropertyKind.OneToMany,
                                    PropertyEntityTypeName = np.PropertyEntityType.AssemblyQualifiedName,
                                    PropertyEntityTableName = foreignTable.Name,
                                    PropertyEntityMember = np.PropertyEntityMember.Name,
                                    ReferencedEntityTypeName = thisTable.ModelTypeName,
                                    ReferencedEntityTableName = thisTable.Name
                                });
                            }
                        }
                    }
                }
            }
        }

        foreach (var tableIndexes in schemaOptions.Indexes.Values)
        {
            foreach (var index in tableIndexes)
            {
                var schemaIndex = new SqliteDbSchemaIndex();
                schemaIndex.TableName = index.SchemaOptions.Tables[index.TableType.AssemblyQualifiedName].Name;
                schemaIndex.IsUnique = index.IsUnique;
                foreach (var column in index.Columns)
                {
                    var schemaIndexCol = new SqliteDbSchemaIndexColumn();
                    schemaIndexCol.Name = column.IndexOptions.SchemaOptions
                        .Tables[column.IndexOptions.TableType.AssemblyQualifiedName]
                        .Columns[column.Member.Name].Name;
                    schemaIndexCol.SortDescending = column.SortDescending;
                    schemaIndexCol.Collation = column.Collation;
                    schemaIndex.Columns.Add(schemaIndexCol);
                }

                schemaIndex.IndexName = string.IsNullOrWhiteSpace(index.IndexName)
                    ? $"index_{schemaIndex.TableName}_{HashIndexSchema(schemaIndex)}"
                    : index.IndexName.Trim();
                result.Indexes.Add(schemaIndex.IndexName, schemaIndex);
            }
        }

        return result;
    }

    private string HashIndexSchema(SqliteDbSchemaIndex index)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{index.TableName}");
        sb.AppendLine($"{index.IsUnique}");
        foreach (var col in index.Columns)
        {
            sb.Append(col.Name);
            sb.Append(col.Collation.GetValueOrDefault());
            sb.AppendLine($"{col.SortDescending}");
        }

        return Convert.ToHexStringLower(Crc32.Hash(Encoding.Unicode.GetBytes(sb.ToString())));
    }
}

public class SqliteTableOptionsBuilder<TTable>
{
    private readonly ISqliteFieldValueSerialization serialization;
    private SqliteTableOptions tableOptions;
    
    public SqliteTableOptionsBuilder(SqliteTableOptions tableOptions, ISqliteFieldValueSerialization serialization)
    {
        this.serialization = serialization;
        this.tableOptions = tableOptions;
    }

    public SqlitePrimaryKeyOptionsBuilder WithPrimaryKey<T>(Expression<Func<TTable, T>> keyField, string name = null,
        bool ascending = true, SqliteLiteConflictAction conflictAction = SqliteLiteConflictAction.Fail)
    {
        if (keyField.Body is MemberExpression exp)
        {
            var options = new SqliteTablePrimaryKeyColumnOptions(tableOptions);
            options.Member = exp.Member;
            options.Name = string.IsNullOrWhiteSpace(name) ? options.Member.Name : name.Trim();
            options.Ascending = ascending;
            options.PrimaryKeyConflictAction = conflictAction;
            options.IsNotNull = true;
            options.IsNotNullConflictAction = conflictAction;
            // ReSharper disable once RedundantDictionaryContainsKeyBeforeAdding
            if (tableOptions.Columns.ContainsKey(options.Member.Name))
                tableOptions.Columns[options.Member.Name] = options;
            else
                tableOptions.Columns.Add(options.Member.Name, options);
            tableOptions.PrimaryKeyColumnOptions = options;
            return new SqlitePrimaryKeyOptionsBuilder(options, serialization);
        }

        throw new InvalidExpressionException();
    }

    public SqliteColumnOptionsBuilder WithColumn<T>(Expression<Func<TTable, T>> field, string name = null)
    {
        if (field.Body is MemberExpression exp)
        {
            var options = new SqliteTableColumnOptions(tableOptions);
            options.Member = exp.Member;
            options.Name = string.IsNullOrWhiteSpace(name) ? options.Member.Name : name.Trim();
            tableOptions.Columns.Add(options.Member.Name, options);
            return new SqliteColumnOptionsBuilder(options, serialization);
        }

        throw new InvalidExpressionException();
    }

    public SqlitePrimaryKeyOptionsBuilder WithAllMembersAsColumns<T>(Expression<Func<TTable, T>> primaryKey,
        bool includeInheritedMembers = true)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        if (includeInheritedMembers) 
            bindingFlags |= BindingFlags.FlattenHierarchy;
        else 
            bindingFlags |= BindingFlags.DeclaredOnly;
        var members = tableOptions.TableType
            .GetMembers(bindingFlags)
            .Where(x => x.MemberType.HasFlag(MemberTypes.Field) || x.MemberType.HasFlag(MemberTypes.Property) &&
                !x.GetCustomAttributes<NotMappedAttribute>().Any()).ToArray();

        foreach (var member in members)
        {
            if (!tableOptions.Columns.ContainsKey(member.Name))
            {
                var options = new SqliteTableColumnOptions(tableOptions);
                options.Member = member;
                options.Name = options.Member.Name;

                var memberType = member.GetValueType();
                if (memberType.IsNotNullable())
                {
                    options.IsNotNull = true;
                    memberType = Nullable.GetUnderlyingType(memberType) ?? memberType;
                    var defaultValue = Activator.CreateInstance(memberType) ?? throw new InvalidOperationException(
                        $"Cannot get default value for non-nullable column {options.Name} of type {memberType.Name}");
                    defaultValue = serialization[memberType]?.Serialize(defaultValue) ?? defaultValue;
                    options.DefaultValueLiteral = defaultValue.ToString();
                }
                
                tableOptions.Columns.Add(options.Member.Name, options);
            }
        }

        return WithPrimaryKey(primaryKey);
    }

    public SqliteTableOptionsBuilder<TTable> WithoutColumn<T>(Expression<Func<TTable, T>> field)
    {
        if (field.Body is MemberExpression exp)
        {
            tableOptions.Columns.Remove(exp.Member.Name);
            return this;
        }
        
        throw new InvalidExpressionException();
    }
    
    public SqliteColumnOptionsBuilder WithColumnChanges<T>(Expression<Func<TTable, T>> field)
    {
        if (field.Body is MemberExpression exp)
        {
            if (tableOptions.Columns.TryGetValue(exp.Member.Name, out var options))
            {
                return new SqliteColumnOptionsBuilder(options, serialization);
            }
        }
        
        throw new InvalidExpressionException();
    }

    public SqliteTableOptionsBuilder<TTable> WithCompositePrimaryKey(params Expression<Func<TTable, object>>[] keyFields)
    {
        var hs = new HashSet<MemberInfo>();
        foreach (var kf in keyFields)
        {
            if (kf.Body is UnaryExpression { Operand: MemberExpression ueo })
                hs.Add(ueo.Member);
            else if (kf.Body is MemberExpression exp)
                hs.Add(exp.Member);
            else
                throw new InvalidExpressionException();
        }
        tableOptions.CompositePrimaryKeyProperties = hs.ToArray();
        return this;
    }

    public SqliteForeignKeyOptionsBuilder<TTable> WithForeignKey(params Expression<Func<TTable, object>>[] fields)
    {
        var options = new SqliteTableForeignKeyOptions(tableOptions);
        var hs = new HashSet<MemberInfo>();
        foreach (var kf in fields)
        {
            if (kf.Body is UnaryExpression { Operand: MemberExpression ueo })
                    hs.Add(ueo.Member);
            else if (kf.Body is MemberExpression exp)
                hs.Add(exp.Member);
            else
                throw new InvalidExpressionException();
        }

        options.ModelProperties = hs.Select(x => new SqliteTableForeignKeyFieldPair
        {
            TableProperty = x
        }).ToArray();
        tableOptions.ForeignKeys.Add(options);
        return new SqliteForeignKeyOptionsBuilder<TTable>(options);
    }
}

public class SqliteColumnOptionsBuilder
{
    private readonly ISqliteFieldValueSerialization serialization;
    private SqliteTableColumnOptions options;
    
    public SqliteColumnOptionsBuilder(SqliteTableColumnOptions options, ISqliteFieldValueSerialization serialization)
    {
        this.serialization = serialization;
        this.options = options;
    }
    
    public SqliteColumnOptionsBuilder IsNotNull(bool isNotNull = true, SqliteLiteConflictAction conflictAction = SqliteLiteConflictAction.Fail)
    {
        options.IsNotNull = isNotNull;
        options.IsNotNullConflictAction = conflictAction;
        return this;
    }
    
    public SqliteColumnOptionsBuilder WithDefaultValue(object defaultValue)
    {
        if (defaultValue is not null)
        {
            var type = options.Member.GetValueType();
            if (type != defaultValue.GetType())
                throw new InvalidDataContractException($"Invalid default value specified on column {options.TableOptions.Name}.{options.Name}");
            defaultValue = serialization[type]?.Serialize(defaultValue) ?? defaultValue;
            options.DefaultValueLiteral = defaultValue.ToString();
        }
        else
            options.DefaultValueLiteral = "NULL";

        return this;
    }    

    public SqliteColumnOptionsBuilder IsUnique(SqliteLiteConflictAction conflictAction = SqliteLiteConflictAction.Fail)
    {
        options.IsUnique = true;
        options.IsUniqueConflictAction = conflictAction;
        return this;
    }

    public SqliteColumnOptionsBuilder UsingCollation(SqliteCollation collation = SqliteCollation.AsciiLowercase)
    {
        options.Collation = collation;
        return this;
    }

    public SqliteColumnOptionsBuilder IsImmutable(bool isImmutable = true)
    {
        options.IsImmutable = isImmutable;
        return this;
    }
    
    public SqliteColumnOptionsBuilder WithNewName(string newName = null)
    {
        options.Name = string.IsNullOrWhiteSpace(newName) ? options.Name : newName.Trim();
        return this;
    }   
}

public class SqlitePrimaryKeyOptionsBuilder : SqliteColumnOptionsBuilder
{
    private SqliteTablePrimaryKeyColumnOptions options;
    
    public SqlitePrimaryKeyOptionsBuilder(SqliteTablePrimaryKeyColumnOptions options, ISqliteFieldValueSerialization serialization)
        : base(options, serialization)
    {
        this.options = options;
    }
    
    public SqlitePrimaryKeyOptionsBuilder IsAutoIncrement(bool enabled = true)
    {
        if (enabled && options.AutoGuid)
            throw new InvalidOperationException(
                $"Cannot enable {nameof(options.AutoIncrement)} because {nameof(options.AutoGuid)} is already enabled. To enable, you must disable {nameof(options.AutoGuid)} first.");
        options.AutoIncrement = enabled;
        return this;
    }
    
    public SqlitePrimaryKeyOptionsBuilder IsAutoGuid(bool enabled = true)
    {
        if (enabled && options.AutoIncrement)
            throw new InvalidOperationException(
                $"Cannot enable {nameof(options.AutoGuid)} because {nameof(options.AutoIncrement)} is already enabled. To enable, you must disable {nameof(options.AutoIncrement)} first.");
        options.AutoGuid = enabled;
        return this;
    }        
}

public class SqliteForeignKeyOptionsBuilder<TTable>
{
    private SqliteTableForeignKeyOptions options;
    
    public SqliteForeignKeyOptionsBuilder(SqliteTableForeignKeyOptions options)
    {
        this.options = options;    
    }
    
    public SqliteForeignKeyOptionsBuilder<TTable> References<TForeignTable>(params Expression<Func<TForeignTable, object>>[] fields) where TForeignTable : class, new()
    {
        var hs = new HashSet<MemberInfo>();
        foreach (var kf in fields)
        {
            if (kf.Body is UnaryExpression { Operand: MemberExpression ueo })
                hs.Add(ueo.Member);
            else if (kf.Body is MemberExpression exp)
                hs.Add(exp.Member);
            else
                throw new InvalidExpressionException();
        }
        options.ForeignTableType = typeof(TForeignTable);
        var fkfItems = hs.ToArray();
        if (fkfItems.Length != options.ModelProperties.Length)
            throw new InvalidExpressionException(
                $"Invalid foreign key specified. {nameof(References)} must select the same number of properties on the foreign table that were selected on the originating table.");
        for(var i=0; i<fkfItems.Length; i++)
        {
            options.ModelProperties[i].ForeignTableProperty = fkfItems[i];
        }
        return this;
    }
    
    public SqliteForeignKeyOptionsBuilder<TTable> HasForeignNavigationProperty<TForeignTable>(Expression<Func<TForeignTable, Lazy<ISqliteQueryable<TTable>>>> listField)
    {
        if (listField.Body is MemberExpression exp)
        {
            var navProp = new SqliteTableForeignKeyNavigationProperty
            {
                Kind = SqliteTableForeignKeyNavigationPropertyKind.OneToMany,
                PropertyEntityType = typeof(TForeignTable), PropertyEntityMember = exp.Member,
                ReferencedEntityType = typeof(TTable), ForeignKeyOptions = options
            };

            options.NavigationProperties.Add($"{exp.Member.DeclaringType.AssemblyQualifiedName}.{exp.Member.Name}", navProp);
            return this;
        }

        throw new InvalidExpressionException();
    }
    
    public SqliteForeignKeyOptionsBuilder<TTable> HasNavigationProperty<TForeignTable>(Expression<Func<TTable, Lazy<TForeignTable>>> detailField)
    {
        if (detailField.Body is MemberExpression exp)
        {
            var navProp = new SqliteTableForeignKeyNavigationProperty
            {
                Kind = SqliteTableForeignKeyNavigationPropertyKind.OneToOne,
                PropertyEntityType = typeof(TTable), PropertyEntityMember = exp.Member,
                ReferencedEntityType = typeof(TForeignTable), ForeignKeyOptions = options
            };

            options.NavigationProperties.Add($"{exp.Member.DeclaringType.AssemblyQualifiedName}.{exp.Member.Name}", navProp);
            return this;
        }

        throw new InvalidExpressionException();
    }
    
    public SqliteForeignKeyOptionsBuilder<TTable> OnUpdate(SqliteForeignKeyAction action)
    {
        options.UpdateAction = action;
        return this;
    }

    public SqliteForeignKeyOptionsBuilder<TTable> OnDelete(SqliteForeignKeyAction action)
    {
        options.DeleteAction = action;
        return this;
    }
}

public class SqliteIndexOptionsBuilder<TTable>
{
    private SqliteIndexOptions indexOptions;
    
    public SqliteIndexOptionsBuilder(SqliteIndexOptions indexOptions)
    {
        this.indexOptions = indexOptions;
    }

    public SqliteIndexOptionsBuilder<TTable> IsUnique(bool unique = true)
    {
        indexOptions.IsUnique = unique;
        return this;
    }

    public SqliteIndexColumnOptionsBuilder WithColumn<T>(Expression<Func<TTable, T>> field)
    {
        if (field.Body is MemberExpression exp)
        {
            var options = new SqliteIndexColumnOptions(indexOptions);
            options.Member = exp.Member;
            indexOptions.Columns.Add(options);
            return new SqliteIndexColumnOptionsBuilder(options);
        }
        
        throw new InvalidExpressionException();
    }
}

public class SqliteIndexColumnOptionsBuilder
{
    private SqliteIndexColumnOptions options;

    public SqliteIndexColumnOptionsBuilder(SqliteIndexColumnOptions options)
    {
        this.options = options;
    }
    
    public SqliteIndexColumnOptionsBuilder UsingCollation(SqliteCollation collation = SqliteCollation.AsciiLowercase)
    {
        options.Collation = collation;
        return this;
    }

    public SqliteIndexColumnOptionsBuilder SortedAscending()
    {
        options.SortDescending = false;
        return this;
    }

    public SqliteIndexColumnOptionsBuilder SortedDescending()
    {
        options.SortDescending = true;
        return this;
    }
}