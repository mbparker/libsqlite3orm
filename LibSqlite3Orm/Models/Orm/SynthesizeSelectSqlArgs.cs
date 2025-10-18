using System.Linq.Expressions;
using System.Reflection;

namespace LibSqlite3Orm.Models.Orm;

public enum SqliteAggregateFunction
{
    Count,
    Sum,
    Total,
    Min,
    Max,
    Avg
}

public class SynthesizeSelectSqlArgs
{
    public SynthesizeSelectSqlArgs(bool loadNavigationProps, Expression filterExpr,
        IReadOnlyList<SqliteSortSpec> sortSpecs, int? skipCount, int? takeCount, SqliteAggregateFunction? aggFunc,
        MemberInfo aggTargetMember)
    {
        LoadNavigationProps = loadNavigationProps;
        FilterExpr = filterExpr;
        SortSpecs = sortSpecs;
        SkipCount = skipCount;
        TakeCount = takeCount;
        AggFunc = aggFunc;
        AggTargetMember = aggTargetMember;
    }
    
    public bool LoadNavigationProps { get; }
    public Expression FilterExpr { get; }
    public IReadOnlyList<SqliteSortSpec> SortSpecs { get; }
    public int? SkipCount { get; }
    public int? TakeCount { get; }
    public SqliteAggregateFunction? AggFunc { get; }
    public MemberInfo AggTargetMember { get; }
}