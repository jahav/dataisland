using JetBrains.Annotations;

namespace Sanctuary.SqlServer;

/// <summary>
/// A configuration of a SQL Database <see cref="SqlDatabaseTenant">tenant</see>
/// in a <see cref="Template"/>.
/// </summary>
[PublicAPI]
public sealed record SqlDatabaseSpec : TenantSpec<SqlDatabaseTenant>
{
    internal SqlDatabaseDataSource? DataSource { get; private set; }

    /// <summary>
    /// Database must be restored using specified backup.
    /// </summary>
    public SqlDatabaseSpec WithDataSource(SqlDatabaseDataSource dataSource)
    {
        return this with { DataSource = dataSource };
    }
}