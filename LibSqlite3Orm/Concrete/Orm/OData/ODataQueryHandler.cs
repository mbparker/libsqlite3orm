using System.Linq.Expressions;
using System.Reflection;
using LibODataParser;
using LibODataParser.FilterExpressions;
using LibODataParser.FilterExpressions.Operators;
using LibODataParser.Sorting;
using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Abstract.Orm.EntityServices;
using LibSqlite3Orm.Abstract.Orm.OData;
using LibSqlite3Orm.Models.Orm.OData;

namespace LibSqlite3Orm.Concrete.Orm.OData;

public class ODataQueryHandler : IODataQueryHandler
{
    private readonly Lazy<IEntityGetter> entityGetter;

    public ODataQueryHandler(Func<ISqliteOrmDatabaseContext, IEntityGetter> entityGetterFactory,
        ISqliteOrmDatabaseContext context)
    {
        entityGetter = new Lazy<IEntityGetter>(() => entityGetterFactory(context));
    }

    public ODataQueryResult<TEntity> ODataQuery<TEntity>(ISqliteConnection connection, string odataQuery) where TEntity : new()
    {
        var parsedQuery = ODataQueryParser.Parse(odataQuery);
        Expression<Func<TEntity, bool>> filterExpression = null;
        if (parsedQuery.Filter is not null)
            filterExpression = BuildFilterExpression<TEntity>(parsedQuery.Filter);
        long? count = null;
        if (parsedQuery.Count == true)
            count = entityGetter.Value.Get<TEntity>(connection, false).Count(filterExpression);
        ISqliteQueryable<TEntity> queryableData = entityGetter.Value.Get<TEntity>(connection, true);
        if (filterExpression is not null)
            queryableData = queryableData.Where(filterExpression);
        ISqliteEnumerable<TEntity> enumerableData = queryableData;
        if (parsedQuery.OrderBy.Count > 0)
            enumerableData = BuildSorting(queryableData, parsedQuery.OrderBy);
        if (parsedQuery.Skip.HasValue)
            enumerableData = enumerableData.Skip(parsedQuery.Skip.Value);
        if (parsedQuery.Top.HasValue)
            enumerableData = enumerableData.Take(parsedQuery.Top.Value);
        return new ODataQueryResult<TEntity>(enumerableData.AsEnumerable(), count);
    }

    private ISqliteOrderedQueryable<TEntity> BuildSorting<TEntity>(ISqliteQueryable<TEntity> queryableData, IReadOnlyList<OrderByClause> orderByItems)
    {
        ISqliteOrderedQueryable<TEntity> orderedQueryable = null;
        var entityType = typeof(TEntity);
        for (var i = 0; i < orderByItems.Count; i++)
        {
            var orderBy = orderByItems[i];
            var first = i == 0;
            var asc = orderBy.Direction == OrderDirection.Ascending;
            var expr = BuildOrderByKeyMemberExpression(entityType, orderBy.Property);
            orderedQueryable = first ? asc ? queryableData.OrderBy(expr) : queryableData.OrderByDescending(expr) :
                asc ? orderedQueryable?.ThenBy(expr) : orderedQueryable?.ThenByDescending(expr);
        }

        return orderedQueryable;
    }

    private Expression BuildOrderByKeyMemberExpression(Type entityType, string memberName)
    {
        var props = memberName.Split('.');
        var param = Expression.Parameter(entityType, "x");
        MemberExpression memExp = null;
        MemberInfo memberInfo = null;

        for (var i = 0; i < props.Length; i++)
        {
            if (i == 0)
            {
                memberInfo = GetMemberInfo(entityType, props[i]);
                memExp = Expression.MakeMemberAccess(param, memberInfo);
            }
            else
            {
                memberInfo = GetMemberInfo(memberInfo.GetValueType(), props[i]);
                memExp = Expression.MakeMemberAccess(memExp, memberInfo);
            }
        }

        var result = Expression.Lambda(memExp, param);
        Console.WriteLine($"{result}");
        return result;
    }

    private Expression<Func<TEntity, bool>> BuildFilterExpression<TEntity>(FilterExpression filter) where TEntity : new()
    {
        var entityType = typeof(TEntity);
        var param = Expression.Parameter(entityType, "x");
        var expression = BuildFilterExpression(entityType, filter);
        return Expression.Lambda<Func<TEntity, bool>>(expression, param);
    }

    private Expression BuildFilterExpression(Type entityType, FilterExpression filter)
    {
        if (filter.Is<FilterBinaryExpression>())
            return BuildBinaryExpression(entityType, filter.As<FilterBinaryExpression>());
        if (filter.Is<FilterFunctionExpression>())
            return BuildFunctionExpression(entityType, filter.As<FilterFunctionExpression>());
        if (filter.Is<FilterLiteralExpression>())
            return BuildLiteralExpression(filter.As<FilterLiteralExpression>());
        if (filter.Is<FilterPropertyExpression>())
            return BuildPropertyExpression(entityType, filter.As<FilterPropertyExpression>());
        if (filter.Is<FilterUnaryExpression>())
            return BuildUnaryExpression(entityType, filter.As<FilterUnaryExpression>());
        throw new InvalidOperationException($"The type of filter expression '{filter.GetType().Name}' is not supported.");
    }
    
    private BinaryExpression BuildBinaryExpression(Type entityType, FilterBinaryExpression expression)
    {
        var expLeft = BuildFilterExpression(entityType, expression.Left);
        var expRight = BuildFilterExpression(entityType, expression.Right);

        if (expRight.NodeType == ExpressionType.Constant && expLeft.NodeType == ExpressionType.MemberAccess)
            expRight = ConvertConstantExpressionIfNeeded(expLeft, expRight);
        else if (expLeft.NodeType == ExpressionType.Constant && expRight.NodeType == ExpressionType.MemberAccess)
            expLeft = ConvertConstantExpressionIfNeeded(expRight, expLeft);
        
        ExpressionType? expressionType = null;
        switch (expression.Operator)
        {
            case BinaryOperator.And:
                expressionType = ExpressionType.AndAlso;
                break;
            case BinaryOperator.Equal:
                expressionType = ExpressionType.Equal;
                break;
            case BinaryOperator.Add:
                expressionType = ExpressionType.Add;
                break;
            case BinaryOperator.Divide:
                expressionType = ExpressionType.Divide;
                break;
            case BinaryOperator.GreaterThan:
                expressionType = ExpressionType.GreaterThan;
                break;
            case BinaryOperator.GreaterThanOrEqual:
                expressionType = ExpressionType.GreaterThanOrEqual;
                break;
            case BinaryOperator.LessThan:
                expressionType = ExpressionType.LessThan;
                break;
            case BinaryOperator.LessThanOrEqual:
                expressionType = ExpressionType.LessThanOrEqual;
                break;
            case BinaryOperator.Modulo:
                expressionType = ExpressionType.Modulo;
                break;
            case BinaryOperator.Multiply:
                expressionType = ExpressionType.Multiply;
                break;
            case BinaryOperator.NotEqual:
                expressionType = ExpressionType.NotEqual;
                break;
            case BinaryOperator.Or:
                expressionType = ExpressionType.Or;
                break;
            case BinaryOperator.Subtract:
                expressionType = ExpressionType.Subtract;
                break;
        }

        if (expressionType is null) throw new InvalidOperationException($"The binary operator {expression.Operator} is not supported.");
        return Expression.MakeBinary(expressionType.Value, expLeft, expRight);
    }
    
    private MethodCallExpression BuildFunctionExpression(Type entityType, FilterFunctionExpression expression)
    {
        var args = expression.Arguments.Select(x => BuildFilterExpression(entityType, x)).ToArray();

        if (new[] { "contains", "startswith", "endswith" }.Contains(expression.FunctionName, StringComparer.OrdinalIgnoreCase))
        {
            if (args.Length == 2 &&
                (args[0].NodeType == ExpressionType.MemberAccess || (args[0].NodeType == ExpressionType.Call &&
                                                                     ((MethodCallExpression)args[0]).Object?.NodeType ==
                                                                     ExpressionType.MemberAccess)) &&
                args[1].NodeType == ExpressionType.Constant || (args[1].NodeType == ExpressionType.Call &&
                                                                ((MethodCallExpression)args[0]).Object?.NodeType ==
                                                                ExpressionType.MemberAccess))
            {
                MemberInfo memberInfo = null;
                if (args[0] is MemberExpression memberExp)
                    memberInfo = memberExp.Member;
                else if (args[0] is MethodCallExpression methodCallExp)
                    memberInfo = (methodCallExp.Object as MemberExpression)?.Member;
                var memberType = memberInfo?.GetValueType();
                if (memberType == typeof(string))
                {
                    var method = memberType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(x =>
                        string.Equals(x.Name, expression.FunctionName, StringComparison.OrdinalIgnoreCase) &&
                        x.GetParameters().Length == 1 &&
                        x.GetParameters()[0].ParameterType == memberType);
                    return Expression.Call(args[0], method, args[1]);
                }

                throw new InvalidOperationException(
                    $"Function '{expression.FunctionName}' is only supported with string values.");
            }
        }

        if (new[] { "toupper", "tolower" }.Contains(expression.FunctionName, StringComparer.OrdinalIgnoreCase))
        {
            if (args.Length == 1 && args[0].NodeType is ExpressionType.MemberAccess or ExpressionType.Constant)
            {
                Type memberType = null;
                if (args[0] is MemberExpression memberExp)
                    memberType = memberExp.Member.GetValueType();
                else if (args[0] is ConstantExpression constantExp)
                    memberType = constantExp.Value?.GetType();
                if (memberType == typeof(string))
                {
                    var method = memberType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(x =>
                        string.Equals(x.Name, expression.FunctionName, StringComparison.OrdinalIgnoreCase) &&
                        x.GetParameters().Length == 0);
                    return Expression.Call(args[0], method);
                }

                throw new InvalidOperationException($"Function '{expression.FunctionName}' is only supported with string values.");
            }               
        }
        
        throw new NotImplementedException($"Function '{expression.FunctionName}' not supported.");
    }
    
    private ConstantExpression BuildLiteralExpression(FilterLiteralExpression expression)
    {
        return Expression.Constant(expression.Value);
    }
    
    private MemberExpression BuildPropertyExpression(Type entityType, FilterPropertyExpression expression)
    {
        var props = expression.PropertyName.Split('.');
        MemberExpression memExp = null;
        MemberInfo memberInfo = null;
        
        for (var i = 0; i < props.Length; i++)
        {
            if (i == 0)
            {
                memberInfo = GetMemberInfo(entityType, props[i]);
                memExp = Expression.MakeMemberAccess(Expression.Parameter(entityType, "x"), memberInfo);
            }
            else
            {
                memberInfo = GetMemberInfo(memberInfo.GetValueType(), props[i]);
                memExp = Expression.MakeMemberAccess(memExp, memberInfo);
            }
        }        
        
        var memberValueType = memberInfo.GetValueType();
        if (memberValueType.IsGenericType && memberValueType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // The bool here is of no consequence. Just getting the name of "Value" without hard coding the string.
            var valueProperty = memberValueType.GetProperty(nameof(Nullable<bool>.Value)) ?? throw new InvalidOperationException("Cannot get Value property on Nullable<> type.");
            return Expression.MakeMemberAccess(memExp, valueProperty);
        }

        Console.WriteLine($"{memExp}");
        return memExp;
    }
    
    private UnaryExpression BuildUnaryExpression(Type entityType, FilterUnaryExpression expression)
    {
        var exp = BuildFilterExpression(entityType, expression.Operand);
        switch (expression.Operator)
        {
            case UnaryOperator.Not:
                return Expression.Not(exp);
            case UnaryOperator.Negate:
                return Expression.Negate(exp);
        }
        
        throw new InvalidOperationException($"The unary operator {expression.Operator} is not supported.");
    }
    
    private Expression ConvertConstantExpressionIfNeeded(Expression memberExpression, Expression constantExpression)
    {
        var memberType = ((MemberExpression)memberExpression).Member.GetValueType() ?? throw new InvalidOperationException("Member type cannot be null.");
        var valueType = ((ConstantExpression)constantExpression).Value?.GetType() ?? throw new InvalidOperationException("Constant value cannot be null.");
        if (!valueType.IsAssignableTo(memberType))
            return Expression.Convert(constantExpression, memberType);
        return constantExpression;
    }

    private MemberInfo GetMemberInfo(Type entityType, string name)
    {
        var member = entityType.GetMember(name, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public).SingleOrDefault();
        return member ?? throw new MemberAccessException($"No Property or Field named {name} on  type {entityType.Name}");
    }
}