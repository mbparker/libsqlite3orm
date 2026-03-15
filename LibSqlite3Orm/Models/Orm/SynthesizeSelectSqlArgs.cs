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

public class SqliteProjectionArgs
{
    public IReadOnlyList<MemberInfo> SelectFields { get; set; } = [];
    public IReadOnlyList<MemberInfo> OmitFields { get; set; } = [];
}

public class SynthesizeSelectSqlArgs
{
    public SynthesizeSelectSqlArgs(bool recursiveLoad, Expression filterExpr,
        IReadOnlyList<SqliteSortSpec> sortSpecs, SqliteProjectionArgs projection, int? skipCount, int? takeCount, SqliteAggregateFunction? aggFunc,
        MemberInfo aggTargetMember)
    {
        RecursiveLoad = recursiveLoad;
        FilterExpr = filterExpr;
        SortSpecs = sortSpecs;
        Projection = projection;
        SkipCount = skipCount;
        TakeCount = takeCount;
        AggFunc = aggFunc;
        AggTargetMember = aggTargetMember;
    }
    
    public bool RecursiveLoad { get; }
    public Expression FilterExpr { get; }
    public IReadOnlyList<SqliteSortSpec> SortSpecs { get; }
    public SqliteProjectionArgs Projection { get; }
    public int? SkipCount { get; }
    public int? TakeCount { get; }
    public SqliteAggregateFunction? AggFunc { get; }
    public MemberInfo AggTargetMember { get; }
}