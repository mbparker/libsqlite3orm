using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Concrete.Orm;

public class SqliteWhereClauseBuilder : ExpressionVisitor, ISqliteWhereClauseBuilder
{
	private readonly IOrmGenerativeLogicTracer generativeLogicTracer;
	private readonly SqliteDbSchema schema;
	private readonly Dictionary<string, ExtractedParameter> extractedParameters = new(StringComparer.OrdinalIgnoreCase);
	private StringBuilder sqlBuilder;
	private SqliteDbSchemaTable table;
	private string currentMemberDbFieldName;
	private string currentMethodCall;
	private object currentConstValue;
	private List<string> currentConstArrayValues = [];
	private bool inContainsCall;

	public SqliteWhereClauseBuilder(IOrmGenerativeLogicTracer generativeLogicTracer, SqliteDbSchema schema)
	{
		this.generativeLogicTracer = generativeLogicTracer;
		this.schema = schema;
	}

	public IReadOnlyDictionary<string, ExtractedParameter> ExtractedParameters => extractedParameters;
	public HashSet<string> ReferencedTables { get; } = new(StringComparer.OrdinalIgnoreCase);

	public string Build(Type entityType, Expression expression)
	{
		table = schema.Tables.Values.Single(x => x.ModelTypeName == entityType.AssemblyQualifiedName);
		generativeLogicTracer.NotifyWhereClauseBuilderVisit(new Lazy<string>($"Parse where predicate for table ({table.Name}): {expression}"));
		extractedParameters.Clear();
		ReferencedTables.Clear();
		sqlBuilder = new StringBuilder();
		Visit(expression);
		return sqlBuilder.ToString();
	}

	protected override Expression VisitUnary(UnaryExpression u)
	{
		switch (u.NodeType)
		{
			case ExpressionType.Not:
				sqlBuilder.Append(" NOT ");
				Visit(u.Operand);
				break;
			case ExpressionType.Convert:
				Visit(u.Operand);
				break;
			default:
				throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported yet");
		}
		
		return u;
	}

	protected override Expression VisitBinary(BinaryExpression b)
	{
		sqlBuilder.Append("(");
		Visit(b.Left);

		switch (b.NodeType)
		{
			case ExpressionType.And:
				sqlBuilder.Append(" AND ");
				break;
			case ExpressionType.AndAlso:
				sqlBuilder.Append(" AND ");
				break;
			case ExpressionType.Or:
				sqlBuilder.Append(" OR ");
				break;
			case ExpressionType.OrElse:
				sqlBuilder.Append(" OR ");
				break;
			case ExpressionType.Equal:
				if (IsNullConstant(b.Right))
				{
					sqlBuilder.Append(" IS ");
				}
				else
				{
					sqlBuilder.Append(" = ");
				}
				break;
			case ExpressionType.NotEqual:
				if (IsNullConstant(b.Right))
				{
					sqlBuilder.Append(" IS NOT ");
				}
				else
				{
					sqlBuilder.Append(" <> ");
				}
				break;
			case ExpressionType.LessThan:
				sqlBuilder.Append(" < ");
				break;
			case ExpressionType.LessThanOrEqual:
				sqlBuilder.Append(" <= ");
				break;
			case ExpressionType.GreaterThan:
				sqlBuilder.Append(" > ");
				break;
			case ExpressionType.GreaterThanOrEqual:
				sqlBuilder.Append(" >= ");
				break;
			default:
				throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported yet");
		}

		Visit(b.Right);
		sqlBuilder.Append(")");
		
		return b;
	}

	protected override Expression VisitConstant(ConstantExpression c)
	{
		IQueryable q = c.Value as IQueryable;

		if (q == null && c.Value == null)
		{
			sqlBuilder.Append("NULL");
		}
		else if (q == null)
		{
			if (BuildingLikeStatement())
			{
				sqlBuilder.Append(" LIKE ");
				if (currentMethodCall.EndsWith(nameof(string.Contains)))
					sqlBuilder.Append($"'%{c.Value}%'");
				else if (currentMethodCall.EndsWith(nameof(string.StartsWith)))
					sqlBuilder.Append($"'{c.Value}%'");
				else if (currentMethodCall.EndsWith(nameof(string.EndsWith)))
					sqlBuilder.Append($"'%{c.Value}'");
				currentMemberDbFieldName = null;
			}
			else if (NeedToParameterizeValue())
			{
				// Were in the middle of parameterizing - get the name, write out the param reference, and save the value.
				var paramName = StoreParameterValueAndReturnName(currentMemberDbFieldName, c.Value);
				currentMemberDbFieldName = null;
				sqlBuilder.Append($":{paramName}");
			}
			else
			{
				switch (Type.GetTypeCode(c.Value.GetType()))
				{
					case TypeCode.Boolean:
						sqlBuilder.Append(((bool)c.Value) ? 1 : 0);
						break;

					case TypeCode.String:
						sqlBuilder.Append("'");
						sqlBuilder.Append(c.Value);
						sqlBuilder.Append("'");
						break;

					case TypeCode.DateTime:
						sqlBuilder.Append("'");
						sqlBuilder.Append(c.Value);
						sqlBuilder.Append("'");
						break;

					case TypeCode.Object:
						currentConstValue = c.Value;
						break;
					default:
						sqlBuilder.Append(c.Value);
						break;
				}
			}
		}

		return c;
	}

	protected override Expression VisitMember(MemberExpression m)
	{
		if (m.Expression is ParameterExpression)
		{
			var memberDbField = GetDbFieldNameForMemberName(m.Member.Name);
			
			if (BuildingInClause() && !ArrayValuesBuilt())
			{
				var elementType = currentConstValue.GetType().GetElementType();
				if (Type.GetTypeCode(elementType) == TypeCode.Object && (elementType?.IsClass ?? false))
				{
					// Array of objects - use expression  to pick the correct field values out of each array element object
					var isStringField =  m.Member.GetValueType() == typeof(string);
					foreach (var val in (IEnumerable)currentConstValue)
					{
						var fieldVal = m.Member.GetValue(val);
						if (isStringField)
							currentConstArrayValues.Add($"'{fieldVal}'");
						else
							currentConstArrayValues.Add($"{fieldVal}");
					}
				}
				else
				{
					// Basic array of values
					var isStrArray =  elementType == typeof(string);
					foreach (var val in (IEnumerable)currentConstValue)
					{
						if (isStrArray)
							currentConstArrayValues.Add($"'{val}'");
						else
							currentConstArrayValues.Add($"{val}");
					}
				}
				
				sqlBuilder.Append(memberDbField);
				sqlBuilder.Append(" IN (");
				sqlBuilder.Append(string.Join(',', currentConstArrayValues));
				sqlBuilder.Append(')');
			}
			else 
			{
				if (inContainsCall && ArrayValuesBuilt())
				{
					inContainsCall = false;
					currentConstArrayValues.Clear();
					currentConstValue = null;
				}
				else
				{
					if (currentConstValue is not null)
						throw new InvalidExpressionException($"Unhandled member access: {m.Member.Name}");
					sqlBuilder.Append(memberDbField);
					currentMemberDbFieldName = memberDbField;
				}
			}
			
			return m;
		} 
		
		if (m.Expression is ConstantExpression ce)
		{
			// Get the value - reading it from the referenced property/field.
			var value = m.Member.GetValue(ce.Value);
			Visit(Expression.Constant(value));
			return m;
		}
		
		if (m.Expression is MemberExpression me)
		{
			if (me.Expression is ConstantExpression ce2)
			{
				var value = m.Member.GetValue(me.Member.GetValue(ce2.Value));
				Visit(Expression.Constant(value));
				return m;
			}

			// x => x.Detail.Value.Member
			if (m.Expression is MemberExpression { Expression: MemberExpression me3 } me2)
			{
				var linkTableName = table.NavigationProperties
					.SingleOrDefault(x => x.PropertyEntityMember == me3.Member.Name)?.ReferencedEntityTableName;
				
				if (!string.IsNullOrWhiteSpace(linkTableName))
				{
					ReferencedTables.Add(linkTableName);
					var colName = schema.Tables[linkTableName].Columns.Values
						.SingleOrDefault(x => x.ModelFieldName == m.Member.Name)?.Name;
					if (!string.IsNullOrWhiteSpace(colName))
					{
						currentMemberDbFieldName = $"{linkTableName}.{colName}";
						sqlBuilder.Append(currentMemberDbFieldName);
					}
				}
				
				Visit(me2);
				return m;
			}

			if (me.Expression is ParameterExpression pe)
			{
				Visit(pe);
				return m;
			}
		}

		throw new NotSupportedException($"The member '{m.Member.Name}' is not supported yet");
	}

	protected override Expression VisitMethodCall(MethodCallExpression mc)
	{
		generativeLogicTracer.NotifyWhereClauseBuilderVisit(new Lazy<string>(() =>
		{
			currentMethodCall = $"{mc.Method.DeclaringType?.AssemblyQualifiedName}.{mc.Method.Name}";
			return $"Visiting Method Call: {currentMethodCall}";
		}));
		if (mc.Method.Name == "Contains")
			inContainsCall = true;
		return base.VisitMethodCall(mc);
	}

	private bool NeedToParameterizeValue()
	{
		return !string.IsNullOrWhiteSpace(currentMemberDbFieldName);
	}

	private bool BuildingInClause()
	{
		return inContainsCall && currentConstValue is not null && currentConstValue.GetType().IsArray;// &&
		       //currentConstArrayValues.Count == 0;
	}
	
	private bool ArrayValuesBuilt()
	{
		return currentConstArrayValues.Count > 0;
	}

	private bool BuildingLikeStatement()
	{
		var strName = typeof(string).AssemblyQualifiedName;
		return currentMethodCall == $"{strName}.{nameof(string.Contains)}" ||
		       currentMethodCall == $"{strName}.{nameof(string.StartsWith)}" ||
		       currentMethodCall == $"{strName}.{nameof(string.EndsWith)}";
	}

	private string GetDbFieldNameForMemberName(string memberName)
	{
		var col = table.Columns.Values.SingleOrDefault(x => x.ModelFieldName == memberName);
		if (col is not null) return $"{table.Name}.{col.Name}";
		throw new InvalidOperationException(
			$"Declaring type of member {memberName} does not exist in the schema.");
	}

	private bool IsNullConstant(Expression exp)
	{
		return exp is ConstantExpression { Value: null };
	}

	private string StoreParameterValueAndReturnName(string dbFieldName, object value)
	{
		var baseName = dbFieldName.Replace(".", string.Empty);
		var name = baseName;
		var dbTableName = dbFieldName.Split('.')[0];
		dbFieldName = dbFieldName.Split('.')[1];
		var uniqueness = 1;
		while (!extractedParameters.TryAdd(name, new ExtractedParameter(name, value, dbTableName, dbFieldName)))
		{
			// Reuse the original parameter if the values are exactly the same.
			// Can't use the == operator here, because the variables are typed as "object" so, it ends up using Object.ReferenceEquals, which will always be false.
			if (extractedParameters[name].Value.Equals(value))
			{
				generativeLogicTracer.NotifyWhereClauseBuilderVisit(new Lazy<string>($"Parameter referenced: {currentMemberDbFieldName} - {name} = {value} ({value.GetType().AssemblyQualifiedName})"));
				return name;
			}

			name = $"{baseName}{uniqueness++}";
		}

		generativeLogicTracer.NotifyWhereClauseBuilderVisit(new Lazy<string>($"Parameter extracted: {currentMemberDbFieldName} - {name} = {value} ({value.GetType().AssemblyQualifiedName})"));
		return name;
	}
}
