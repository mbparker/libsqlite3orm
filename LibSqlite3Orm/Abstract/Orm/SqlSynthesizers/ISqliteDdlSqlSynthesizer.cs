namespace LibSqlite3Orm.Abstract.Orm.SqlSynthesizers;

public interface ISqliteDdlSqlSynthesizer
{
    string SynthesizeCreate(string objectNameInSchema, string newObjectName = null);
    
    string SynthesizeDrop(string objectName);
}