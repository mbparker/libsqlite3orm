using LibSqlite3Orm.Abstract;

namespace LibSqlite3Orm.Concrete;

public class SqliteFileOperations : ISqliteFileOperations
{
    public void DeleteFile(string path)
    {
        if (!FileExists(path)) throw new ArgumentException("File does not exist");
        File.Delete(path);
    }

    public bool FileExists(string path)
    {
        if (path is null) throw new ArgumentNullException(nameof(path));
        return File.Exists(path);
    }
}