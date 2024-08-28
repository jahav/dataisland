using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class DataAccessProfile
{
    /// <summary>
    /// Key: data access. Value: tenant name.
    /// </summary>
    internal readonly Dictionary<Type, string> _dataAccess = new();

    /// <summary>
    /// Key: tenant name. Value: component name and data source.
    /// </summary>
    internal readonly Dictionary<string, TenantConfig> _tenants = new();

    public DataAccessProfile AddDataAccess<TDataAccess>(string tenantName)
        where TDataAccess : class
    {
        _dataAccess.Add(typeof(TDataAccess), tenantName);
        return this;
    }

    public ITenantConfig<TTenant> AddTenant<TTenant>(string tenantName, string componentName)
    {
        _tenants.Add(tenantName, new TenantConfig(typeof(TTenant), componentName, null));
        return new InternalBuilder<TTenant>(this, tenantName);
    }

    private class InternalBuilder<TTenant>(DataAccessProfile profile, string tenantName)
        : ITenantConfig<TTenant>
    {
        public ITenantConfig<TTenant> WithDataSource<TDataSource>(TDataSource dataSource)
        {
            profile._tenants[tenantName] = profile._tenants[tenantName] with { DataSource = dataSource };
            return this;
        }
    }
}