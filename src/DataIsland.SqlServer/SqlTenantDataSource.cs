namespace DataIsland.SqlServer;

/// <summary>
/// A data source object specifying the data that are used to fill the newly created tenant database with data.
/// It uses a SQL Server database backup as the source.
/// </summary>
public class SqlDatabaseDataSource
{
    /// <summary>
    /// Data source is an empty database.
    /// </summary>
    public SqlDatabaseDataSource()
    {
    }

    internal string? Path { get; private init; }

    internal int? File { get; private init; }

    /// <summary>
    /// The data source is a backup of a database stored on a single device (e.g. file on a hard drive).
    /// </summary>
    /// <param name="path">Path to the disk containing the backup, most likely a path to a .bak file.</param>
    /// <param name="file">File in the disk. Disk can contain multiple files.</param>
    public SqlDatabaseDataSource FromDisk(string path, int file = 1)
    {
        return new SqlDatabaseDataSource
        {
            Path = path,
            File = file,
        };
    }
}
