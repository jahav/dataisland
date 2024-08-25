namespace Sanctuary.SqlServer;

public record SqlDatabaseTenant(string TenantName, string ConnectionString, SqlServerComponent Component, string DatabaseName);