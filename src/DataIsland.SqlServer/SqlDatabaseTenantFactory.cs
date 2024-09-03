using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataIsland.SqlServer;

public sealed class SqlDatabaseTenantFactory : ITenantFactory<SqlDatabaseTenant, SqlServerComponent, SqlDatabaseSpec>
{
    private readonly string _basePath;

    public SqlDatabaseTenantFactory(string basePath)
    {
        _basePath = basePath;
    }

    /// <inheritdoc />
    public async Task<SqlDatabaseTenant> AddTenantAsync(SqlServerComponent component, SqlDatabaseSpec spec)
    {
        // Use connections string
        var tenantDbName = Guid.NewGuid().ToString();

        // Because connection string of component doesn't differ, it will be pooled.
        using var connection = new SqlConnection(component.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        if (!spec.HasDataSource)
        {
            command.CommandText = $"CREATE DATABASE [{EscapeDbName(tenantDbName)}]";
        }
        else
        {
            var escapedPath = EscapePath(spec.DataSource);
            var files = GetLogicalFiles(connection, spec.DataSource, spec.File.Value);
            var cmd = new StringBuilder($"""
                    DBCC TRACEON(1800, -1);
                    RESTORE DATABASE [{EscapeDbName(tenantDbName)}]
                        FROM DISK = '{escapedPath}'
                        WITH
                            FILE = 1,
""");
            cmd.AppendLine();
            foreach (var file in files)
            {
                var filePath = Path.Combine(_basePath, $"{file.LogicalName}-{tenantDbName}{file.Extension}");
                cmd.Append(' ', 8).AppendLine(@$"MOVE N'{file.LogicalName}' TO N'{EscapePath(filePath)}',");
            }

            // Need to set RECOVERY, otherwise database will be in RESTORING state and can't be changed
            // to single user model for deletion.
            cmd.Append(' ', 8).Append(@"RECOVERY");

            command.CommandText = cmd.ToString();
        }

        command.ExecuteNonQuery();

        if (spec.MaxDop is { } maxDop)
            await SetMaxDopAsync(connection, tenantDbName, maxDop);

        connection.Close();

        var tenantConnectionString = new SqlConnectionStringBuilder(component.ConnectionString)
        {
            InitialCatalog = tenantDbName
        }.ConnectionString;

        return new SqlDatabaseTenant(tenantConnectionString, component, tenantDbName);
    }

    /// <inheritdoc />
    public Task<bool> RemoveTenantAsync(SqlServerComponent component, SqlDatabaseTenant tenant)
    {
        using var connection = new SqlConnection(component.ConnectionString);
        connection.Open();

        var dropConnections = connection.CreateCommand();
        dropConnections.CommandText = $"ALTER DATABASE [{tenant.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        dropConnections.ExecuteNonQuery();

        var createDatabaseCommand = connection.CreateCommand();
        createDatabaseCommand.CommandText = $"DROP DATABASE [{tenant.DatabaseName}]";
        createDatabaseCommand.ExecuteNonQuery();
        connection.Close();
        return Task.FromResult(true);
    }

    private static List<BackupFile> GetLogicalFiles(SqlConnection connection, string diskPath, int file)
    {
        // TODO: Don't do on every tenant
        var logicalFiles = new List<BackupFile>();
        var query = connection.CreateCommand();
        query.CommandText = $"""
                RESTORE FILELISTONLY
                FROM DISK = N'{EscapePath(diskPath)}'
                WITH
                    FILE = {file},
                    NOUNLOAD
""";
        using var reader = query.ExecuteReader();
        var logicalNameOrdinal = reader.GetOrdinal("LogicalName");
        var typeOrdinal = reader.GetOrdinal("Type");
        while (reader.Read())
        {
            var logicalName = reader.GetString(logicalNameOrdinal);
            var extension = reader.GetString(typeOrdinal) switch
            {
                "D" => ".mdf",
                "L" => ".ldf",
                _ => throw new UnreachableException(),
            };
            logicalFiles.Add(new BackupFile(logicalName, extension));
        }

        return logicalFiles;
    }

    private readonly record struct BackupFile(string LogicalName, string Extension);

    private static string EscapePath(string path)
    {
        return path.Replace("'", "''");
    }

    private static string EscapeDbName(string databaseName)
    {
        return databaseName.Replace("[", "[[").Replace("]", "]]");
    }

    private static async Task SetMaxDopAsync(DbConnection connection, string dbName, int maxDop)
    {
        using var setMaxDop = connection.CreateCommand();
        setMaxDop.CommandText = $"""
                                 USE [{EscapeDbName(dbName)}];
                                 DECLARE @currentMaxDop int =
                                     (
                                         SELECT CONVERT(int, [value])
                                         FROM sys.database_scoped_configurations
                                         WHERE [name] = 'MAXDOP'
                                     );
                                 IF (@currentMaxDop != {maxDop})
                                 BEGIN
                                     ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = {maxDop};
                                 END;
                                 """;
        await setMaxDop.ExecuteNonQueryAsync();
    }
}
