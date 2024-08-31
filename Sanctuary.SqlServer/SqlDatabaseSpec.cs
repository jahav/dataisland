using System;
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

    internal int? MaxDop { get; private set; }

    /// <summary>
    /// Database must be restored using specified backup.
    /// </summary>
    public SqlDatabaseSpec WithDataSource(SqlDatabaseDataSource dataSource)
    {
        return this with { DataSource = dataSource };
    }

    /// <summary>
    /// The tenant will use the provided maximum degree of parallelism.
    /// </summary>
    /// <param name="maxDop">Maximum number of cores used to execute a query. 0 means SQL Server determines optimal value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Value is less than 0.</exception>
    public SqlDatabaseSpec WithMaxDop(int maxDop)
    {
        if (maxDop < 0)
            throw new ArgumentOutOfRangeException(nameof(maxDop));

        return this with { MaxDop = maxDop };
    }
}