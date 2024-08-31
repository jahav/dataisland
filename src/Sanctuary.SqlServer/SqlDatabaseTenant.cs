namespace Sanctuary.SqlServer;

public record SqlDatabaseTenant(string ConnectionString, SqlServerComponent Component, string DatabaseName);