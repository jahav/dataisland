using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Sanctuary.SqlServer;

public sealed class SqlDatabaseTenantFactory : ITenantFactory<SqlDatabaseTenant, SqlDatabaseDataSource>
{
    private readonly SqlServerComponent _component;
    private readonly string _basePath;
    private readonly SqlDatabaseDataSource _dataSource;

    public SqlDatabaseTenantFactory(SqlServerComponent component, string basePath, SqlDatabaseDataSource dataSource)
    {
        _component = component;
        _basePath = basePath;
        _dataSource = dataSource;
    }

    /// <inheritdoc />
    public Task<SqlDatabaseTenant> AddTenantAsync(string tenantName, SqlDatabaseDataSource? dataSource)
    {
        // Use connections string
        var tenantDbName = Guid.NewGuid().ToString();

        // Because connection string of component doesn't differ, it will be pooled.
        using var connection = new SqlConnection(_component.ConnectionString);
        connection.Open();

        var command = connection.CreateCommand();
        dataSource ??= _dataSource;
        if (dataSource.Path is null || dataSource.File is null)
        {
            command.CommandText = $"CREATE DATABASE [{EscapeDbName(tenantDbName)}]";
        }
        else
        {
            var escapedPath = EscapePath(dataSource.Path);
            var files = GetLogicalFiles(connection, dataSource.Path, dataSource.File.Value);
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
        connection.Close();

        var tenantConnectionString = new SqlConnectionStringBuilder(_component.ConnectionString)
        {
            InitialCatalog = tenantDbName
        }.ConnectionString;

        return Task.FromResult(new SqlDatabaseTenant(tenantName, tenantConnectionString, _component, tenantDbName));
    }

    /// <inheritdoc />
    public Task RemoveTenantAsync(SqlDatabaseTenant tenant)
    {
        using var connection = new SqlConnection(_component.ConnectionString);
        connection.Open();
        
        var dropConnections = connection.CreateCommand();
        dropConnections.CommandText = $"ALTER DATABASE [{tenant.DatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        dropConnections.ExecuteNonQuery();

        var createDatabaseCommand = connection.CreateCommand();
        createDatabaseCommand.CommandText = $"DROP DATABASE [{tenant.DatabaseName}]";
        createDatabaseCommand.ExecuteNonQuery();
        connection.Close();
        return Task.CompletedTask;
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

    async Task<object> ITenantFactory.AddTenantAsync(string tenantName, object? dataSource)
    {
        return await AddTenantAsync(tenantName, (SqlDatabaseDataSource?)dataSource);
    }

    async Task ITenantFactory.RemoveTenantAsync(object tenant)
    {
        await RemoveTenantAsync((SqlDatabaseTenant)tenant);
    }
}