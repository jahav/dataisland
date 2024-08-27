using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

internal record TenantConfig(string ComponentName, object? DataSource);


[PublicAPI]
public class DataAccessProfile
{
    /// <summary>
    /// Map from data access to tenant name.
    /// </summary>
    internal readonly Dictionary<Type, string> _dataAccess = new();

    /// <summary>
    /// Map of all tenants to data accesses
    /// </summary>
    internal readonly Dictionary<string, TenantConfig> _tenants = new();

    public IDataAccessBuilder<TDataAccess> AddDataAccess<TDataAccess>(
        string tenantName = "Default",
        string componentName = "Default")
        where TDataAccess : class
    {
        _dataAccess.Add(typeof(TDataAccess), tenantName);
        _tenants.Add(tenantName, new TenantConfig(componentName, null));

        return new InternalBuilder<TDataAccess>(this, tenantName);
    }

    private class InternalBuilder<TDataAccess>(DataAccessProfile profile, string tenantName)
        : IDataAccessBuilder<TDataAccess>
    {
        public IDataAccessBuilder<TDataAccess> WithDataSource<TDataSource>(TDataSource dataSource)
        {
            profile._tenants[tenantName] = profile._tenants[tenantName] with { DataSource = dataSource };
            return this;
        }
    }
}