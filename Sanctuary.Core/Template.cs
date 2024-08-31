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
    internal readonly Dictionary<string, ITenantSpec> _tenants;

    /// <summary>
    /// Key: component name. Value: component specification.
    /// </summary>
    internal readonly Dictionary<string, IComponentSpec> _components;

    internal Template()
    {
        _dataAccess = new();
        _tenants = new();
        _components = new();
    }

    internal Template(Template original)
    {
        _dataAccess = new Dictionary<Type, string>(original._dataAccess);
        _tenants = new Dictionary<string, ITenantSpec>(original._tenants);
        _components = new Dictionary<string, IComponentSpec>(original._components);
    }

    public Template AddDataAccess<TDataAccess>(string tenantName)
        where TDataAccess : class
    {
        _dataAccess.Add(typeof(TDataAccess), tenantName);
        return this;
    }

    public void AddTenant<TTenant, TTenantSpec>(string tenantName, string componentName, Func<TTenantSpec, TTenantSpec>? config = null)
        where TTenantSpec : TenantSpec<TTenant>, new()
    {
        var spec = new TTenantSpec
        {
            ComponentName = componentName
        };
        spec = config?.Invoke(spec) ?? spec;
        _tenants.Add(tenantName, spec);
    }

    public void AddComponent<TComponent, TComponentSpec>(string componentName, Func<TComponentSpec, TComponentSpec>? config = null)
        where TComponentSpec : ComponentSpec<TComponent>, new()
    {
        var spec = new TComponentSpec();
        spec = config?.Invoke(spec) ?? spec;
        _components.Add(componentName, spec);
    }
}