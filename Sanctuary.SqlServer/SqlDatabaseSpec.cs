using JetBrains.Annotations;

namespace Sanctuary.SqlServer;

/// <summary>
/// A configuration
/// </summary>
[PublicAPI]
public sealed record SqlDatabaseSpec() : TenantSpec<SqlDatabaseTenant>()
{
    internal int? MaxDop { get; private set; }
    
    internal SqlDatabaseDataSource? DataSource { get; private set; }

    /// <summary>
    /// Set maximum degree of parallelization.
    /// </summary>
    public SqlDatabaseSpec WithMaxDop(int maxDop)
    {
        return this with { MaxDop = maxDop };
    }

    /// <summary>
    /// Database must be restored using specified backup.
    /// </summary>
    public SqlDatabaseSpec WithDataSource(SqlDatabaseDataSource dataSource)
    {
        return this with { DataSource = dataSource };
    }
}