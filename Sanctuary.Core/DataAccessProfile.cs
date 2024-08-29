using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class LogicalView
{
    /// <summary>
    /// Key: data access. Value: tenant name.
    /// </summary>
    internal readonly Dictionary<Type, string> _dataAccess;

    /// <summary>
    /// Key: tenant name. Value: component name and data source.
    /// </summary>
    internal readonly Dictionary<string, TenantConfig> _tenants;

    internal LogicalView()
    {
        _dataAccess = new();
        _tenants = new();
    }

    internal LogicalView(LogicalView original)
    {
        _dataAccess = new Dictionary<Type, string>(original._dataAccess);
        _tenants = new Dictionary<string, TenantConfig>(original._tenants);
    }

    public LogicalView AddDataAccess<TDataAccess>(string tenantName)
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

    private class InternalBuilder<TTenant>(LogicalView logicalView, string tenantName)
        : ITenantConfig<TTenant>
    {
        public ITenantConfig<TTenant> WithDataSource<TDataSource>(TDataSource dataSource)
        {
            logicalView._tenants[tenantName] = logicalView._tenants[tenantName] with { DataSource = dataSource };
            return this;
        }
    }
}