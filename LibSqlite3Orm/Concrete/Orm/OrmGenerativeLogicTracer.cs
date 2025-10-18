using LibSqlite3Orm.Abstract;
using LibSqlite3Orm.Abstract.Orm;
using LibSqlite3Orm.Models.Orm.Events;

namespace LibSqlite3Orm.Concrete.Orm;

public class OrmGenerativeLogicTracer : IOrmGenerativeLogicTracer
{
    public event EventHandler<SqlStatementExecutingEventArgs> SqlStatementExecuting;
    public event EventHandler<GenerativeLogicTraceEventArgs> WhereClauseBuilderVisit;
    
    public void NotifySqlStatementExecuting(string sqlStatement, ISqliteParameterCollectionDebug parameters)
    {
        SqlStatementExecuting?.Invoke(this, new SqlStatementExecutingEventArgs(sqlStatement, parameters));
    }

    public void NotifyWhereClauseBuilderVisit(Lazy<string> message)
    {
        WhereClauseBuilderVisit?.Invoke(this, new GenerativeLogicTraceEventArgs(message));
    }
}