using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class Template
{
    /// <summary>
    /// Key: data access. Value: tenant name.
    /// </summary>
    internal readonly Dictionary<Type, string> _dataAccess;

    /// <summary>
    /// Key: tenant name. Value: tenant specification.
    /// </summary>
    internal readonly Dictionary<string, TenantSpec> _tenants;

    /// <summary>
    /// Key: component name. Value: component specification.
    /// </summary>
    internal readonly Dictionary<string, ComponentSpec> _components;

    internal Template()
    {
        _dataAccess = new();
        _tenants = new();
        _components = new();
    }

    internal Template(Template original)
    {
        _dataAccess = new Dictionary<Type, string>(original._dataAccess);
        _tenants = new Dictionary<string, TenantSpec>(original._tenants);
        _components = new Dictionary<string, ComponentSpec>(original._components);
    }

    public Template AddDataAccess<TDataAccess>(string tenantName)
        where TDataAccess : class
    {
        _dataAccess.Add(typeof(TDataAccess), tenantName);
        return this;
    }

    public ITenantSpecBuilder<TTenant> AddTenant<TTenant>(string tenantName, string componentName)
    {
        _tenants.Add(tenantName, new TenantSpec(typeof(TTenant), componentName, null));
        return new TenantSpecBuilder<TTenant>(this, tenantName);
    }

    public void AddComponent<TComponent>(string componentName)
    {
        _components.Add(componentName, new ComponentSpec(typeof(TComponent)));
    }

    private class TenantSpecBuilder<TTenant>(Template template, string tenantName)
        : ITenantSpecBuilder<TTenant>
    {
        public ITenantSpecBuilder<TTenant> WithDataSource<TDataSource>(TDataSource dataSource)
        {
            template._tenants[tenantName] = template._tenants[tenantName] with { DataSource = dataSource };
            return this;
        }
    }
}