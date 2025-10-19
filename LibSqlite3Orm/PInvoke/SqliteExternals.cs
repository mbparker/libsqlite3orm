using System.Runtime.InteropServices;
using System.Text;
using LibSqlite3Orm.PInvoke.Types.Enums;
using LibSqlite3Orm.PInvoke.Types.Exceptions;

namespace LibSqlite3Orm.PInvoke;

public static partial class SqliteExternals
{
	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_threadsafe",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern int Threadsafe();

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_open", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_open_v2",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Open2([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db,
		int flags, [MarshalAs(UnmanagedType.LPStr)] string zvfs);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_enable_load_extension",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult EnableLoadExtension(IntPtr db, int onoff);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_close", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Close(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_close_v2",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Close2(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_initialize",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Initialize();

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_shutdown",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Shutdown();

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_config", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Config(SqliteConfigOption option);
	
	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_db_config", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult DbConfig(IntPtr db, SqliteDbConfigOption option, int value, IntPtr outValue);	

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_busy_timeout",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult BusyTimeout(IntPtr db, int milliseconds);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_changes",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern int Changes(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_prepare_v2",
		CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Prepare2(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql,
		int numBytes, out IntPtr stmt, IntPtr pzTail);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Step(IntPtr stmt);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_reset", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Reset(IntPtr stmt);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult Finalize(IntPtr stmt);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_last_insert_rowid", CallingConvention = CallingConvention.Cdecl)]
	public static extern long LastInsertRowId(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_errmsg", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr ErrorMsgInternal(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_parameter_index", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindParameterIndex(IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string name);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_null", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindNull(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindInt(IntPtr stmt, int index, int val);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindInt64(IntPtr stmt, int index, long val);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_double", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindDouble(IntPtr stmt, int index, double val);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_text", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindText(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPStr)] string val, int n,
		IntPtr free);
	
	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_text16", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindTextUtf16(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string val, int n,
		IntPtr free);	

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_bind_blob", CallingConvention = CallingConvention.Cdecl)]
	public static extern int BindBlob(IntPtr stmt, int index, byte[] val, int n, IntPtr free);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
	public static extern int ColumnCount(IntPtr stmt);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_name", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr ColumnNameInternal(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteDataType ColumnType(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_int", CallingConvention = CallingConvention.Cdecl)]
	public static extern int ColumnInt(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
	public static extern long ColumnInt64(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
	public static extern double ColumnDouble(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr ColumnTextInternal(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
	static extern IntPtr ColumnBlobInternal(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
	public static extern int ColumnBytes(IntPtr stmt, int index);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_errcode", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult ErrorCode(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_extended_errcode", CallingConvention = CallingConvention.Cdecl)]
	public static extern SqliteResult ExtendedErrCode(IntPtr db);

	[DllImport(SqliteConstants.LibraryPath, EntryPoint = "sqlite3_libversion_number", CallingConvention = CallingConvention.Cdecl)]
	public static extern int LibVersionNumber();
}

public static partial class SqliteExternals
{
	public static IntPtr Prepare2(IntPtr db, string query)
	{
		IntPtr stmt;
		var r = Prepare2(db, query, Encoding.UTF8.GetByteCount(query), out stmt, IntPtr.Zero);

		if (r != SqliteResult.OK)
		{
			throw new SqliteException(r, ExtendedErrCode(db), ErrorMsg(db));
		}

		return stmt;
	}

	public static string ColumnText(IntPtr stmt, int index)
	{
		return Marshal.PtrToStringUTF8(ColumnTextInternal(stmt, index));
	}

	public static byte[] ColumnBlob(IntPtr stmt, int index)
	{
		int length = ColumnBytes(stmt, index);
		var result = new byte[length];
		if (length > 0)
			Marshal.Copy(ColumnBlobInternal(stmt, index), result, 0, length);
		return result;
	}
    
	public static string ColumnName(IntPtr stmt, int index)
	{
		return Marshal.PtrToStringUTF8(ColumnNameInternal(stmt, index));
	}
    
	public static string ErrorMsg(IntPtr db)
	{
		return Marshal.PtrToStringUTF8(ErrorMsgInternal(db));
	}

	public static void SetForeignKeyEnforcement(IntPtr db, bool enabled)
	{
		var outVal = Marshal.AllocHGlobal(sizeof(int));
		try
		{
			var ret = DbConfig(db, SqliteDbConfigOption.EnableForeignKeys, enabled ? 1 : 0, outVal);
			if (ret != SqliteResult.OK)
				throw new SqliteException(ret, "Cannot set foreign key enforcement: Library error.");
			var realOutVal = Marshal.ReadInt32(outVal);
			if (realOutVal == 1 != enabled)
				throw new ApplicationException(
					"Cannot set foreign key enforcement: Value after change is not as expected.");
		}
		finally
		{
			Marshal.FreeHGlobal(outVal);
		}
	}
}