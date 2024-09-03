namespace DataIsland;

/// <summary>
/// A base tenant for relational databases.
/// </summary>
/// <param name="ConnectionString">Connection string to the database.</param>
/// <param name="DatabaseName">Name of the database (unescaped).</param>
public abstract record AdoNetDatabaseTenant(string ConnectionString, string DatabaseName)
{
}
