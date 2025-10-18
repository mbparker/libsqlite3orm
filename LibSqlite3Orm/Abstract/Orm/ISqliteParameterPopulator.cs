using LibSqlite3Orm.Models.Orm;

namespace LibSqlite3Orm.Abstract.Orm;

public interface ISqliteParameterPopulator
{
    void Populate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection, T entity);
    
    void Populate<T>(DmlSqlSynthesisResult synthesisResult,
        ISqliteParameterCollection parameterCollection);    
}