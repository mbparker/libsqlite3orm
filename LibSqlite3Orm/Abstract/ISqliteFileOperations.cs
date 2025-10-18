namespace LibSqlite3Orm.Abstract;

public interface ISqliteFileOperations
{
    void DeleteFile(string path);
    bool FileExists(string path);
}