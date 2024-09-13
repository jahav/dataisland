using JetBrains.Annotations;
using Testcontainers.MsSql;

namespace DataIsland.SqlServer;

/// <summary>
/// A class that provides various component pools for a <see cref="SqlServerComponent"/>.
/// </summary>
[PublicAPI]
public static class SqlServerComponentFactory
{
    /// <summary>
    /// Create a new pool that contains only one existing external SQL server.
    /// </summary>
    /// <param name="connectionString">A connection string to SQL server that will be used to manipulate the
    ///     component (add/remove databases, load data). This connection string is also used as a template
    ///     for connection strings for each tenant (the database in the string will be adjusted for a tenant).
    /// </param>
    public static IComponentPool<SqlServerComponent, SqlServerSpec> ExistingSqlServer(string connectionString)
    {
        return new OneSqlServer(connectionString);
    }

    /// <summary>
    /// <para>
    /// Return a pool of SQL servers instantiated in a docker.
    /// </para>
    /// <para>
    /// Simplest usage:
    /// <example>
    /// <code>
    /// var sqlServerPool = SqlServerComponentFactory.Docker(new MsSqlBuilder().Build());
    /// </code>
    /// </example>
    /// For more details about how to instantiate and set the docker image, see
    /// <a href="https://dotnet.testcontainers.org/modules/mssql/">TestContainers MsSql documentation</a>.
    /// </para>
    /// </summary>
    /// <param name="msSqlContainers">SQL Server containers.</param>
    public static IComponentPool<SqlServerComponent, SqlServerSpec> Docker(params MsSqlContainer[] msSqlContainers)
    {
        return new DockerSqlServerPool(msSqlContainers);
    }
}
