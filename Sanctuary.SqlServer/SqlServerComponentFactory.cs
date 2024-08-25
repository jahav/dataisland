using JetBrains.Annotations;

namespace Sanctuary.SqlServer;

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
    /// <param name="name">Name of the component. All tenants must use this name, because pool doesn't contain any other component.</param>
    public static IComponentPool<SqlServerComponent> ExistingSqlServer(string connectionString, string name = "DefaultComponent")
    {
        return new OneSqlServer(name, connectionString);
    }
}