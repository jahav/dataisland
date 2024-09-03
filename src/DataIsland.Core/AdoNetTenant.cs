using System;

namespace DataIsland;

/// <summary>
/// A base tenant for relational databases.
/// </summary>
public abstract record AdoNetTenant
{
    /// <summary>
    /// A base tenant for relational databases.
    /// </summary>
    /// <param name="connectionString">Connection string to the database. Can't be empty.</param>
    /// <param name="databaseName">Name of the database (unescaped). Can't be empty.</param>
    protected AdoNetTenant(string connectionString, string databaseName)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Connection string can't be empty.", nameof(connectionString));

        if (string.IsNullOrEmpty(databaseName))
            throw new ArgumentException("Database name can't be empty.", nameof(databaseName));

        ConnectionString = connectionString;
        DatabaseName = databaseName;
    }

    /// <summary>Connection string to the database. Can't be empty.</summary>
    public string ConnectionString { get; }

    /// <summary>Name of the database (unescaped). Can't be empty.</summary>
    public string DatabaseName { get; }
}
