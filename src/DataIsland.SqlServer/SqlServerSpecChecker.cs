using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace DataIsland.SqlServer;

internal static class SqlServerSpecChecker
{
    internal static async Task<string?> CheckAsync(SqlServerSpec componentSpec, string connectionString)
    {
        if (componentSpec.Collation is not null)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT SERVERPROPERTY('Collation')";
            var collation = (string?)await command.ExecuteScalarAsync();
            connection.Close();
            if (collation != componentSpec.Collation)
                return $"Unable to find a server with collation {componentSpec.Collation}.";
        }

        if (componentSpec.ClrEnabled is not null)
        {
            var desiredClrEnabled = componentSpec.ClrEnabled.Value;
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = """
                                  DECLARE @T TABLE (
                                      [name] nvarchar(35),
                                      [minimum] int,
                                      [maximum] int,
                                      [config_value] int,
                                      [run_value] int);
                                  INSERT @T EXEC sp_configure 'clr enabled';
                                  SELECT run_value FROM @T;
                                  """;
            var clrConfigValue = (int?)await command.ExecuteScalarAsync();
            var actualClrEnabled = clrConfigValue is not null && clrConfigValue.Value != 0;
            connection.Close();
            if (actualClrEnabled != desiredClrEnabled)
                return $"Unable to find a server with clr {(desiredClrEnabled ? "enabled" : "disabled")}.";
        }

        if (componentSpec.LinkedServerNames.Count > 0)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            var specifiedNames = componentSpec.LinkedServerNames;
            var dbNames = await GetDbLinkedServerNamesAsync(connection);
            var dbHasAllLinkedServers = componentSpec.LinkedServerNames.IsSubsetOf(dbNames);
            if (!dbHasAllLinkedServers)
            {
                var specifiedList = specifiedNames.ToSysnameList();
                var dbList = dbNames.ToSysnameList();
                return $"Server doesn't contain all required linked servers (specified: {specifiedList}, actual: {dbList}).";
            }
        }

        return null;
    }

    private static async Task<HashSet<string>> GetDbLinkedServerNamesAsync(SqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
                              DECLARE @LinkedServers TABLE (
                                  [SRV_NAME] sysname,
                                  [SRV_PROVIDERNAME] nvarchar(128),
                                  [SRV_PRODUCT] nvarchar(128),
                                  [SRV_DATASOURCE] nvarchar(4000),
                                  [SRV_PROVIDERSTRING] nvarchar(4000),
                                  [SRV_LOCATION] nvarchar(4000),
                                  [SRV_CAT] sysname NULL);
                              INSERT @LinkedServers EXEC sp_linkedservers;
                              SELECT [SRV_NAME] FROM @LinkedServers;
                              """;
        using var reader = await command.ExecuteReaderAsync();
        var srvNameOrdinal = reader.GetOrdinal("SRV_NAME");
        var dbLinkedServerNames = new HashSet<string>();
        while (await reader.ReadAsync())
        {
            // Never null, therefore no need to deal with DBNull.Value to null conversion.
            dbLinkedServerNames.Add(reader.GetString(srvNameOrdinal));
        }

        return dbLinkedServerNames;
    }
}
