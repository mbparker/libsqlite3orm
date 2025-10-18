using System.Linq.Expressions;

namespace LibSqlite3Orm.Models.Orm;

public class SynthesizeDeleteSqlArgs
{
    public SynthesizeDeleteSqlArgs(Expression filterExpr)
    {
        FilterExpr = filterExpr;
    }
    
    public Expression FilterExpr { get; }
}