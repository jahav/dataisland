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

        return null;
    }
}
