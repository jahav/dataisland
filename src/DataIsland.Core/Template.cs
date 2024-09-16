using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace DataIsland;

[PublicAPI]
public class Template
{
    private readonly List<Func<IReadOnlyCollection<Tenant>, CancellationToken, Task>> _afterInitHooks;

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
        _dataAccess = [];
        _tenants = [];
        _components = [];
        _afterInitHooks = [];
    }

    internal Template(Template original)
    {
        _dataAccess = new Dictionary<Type, string>(original._dataAccess);
        _tenants = new Dictionary<string, ITenantSpec>(original._tenants);
        _components = new Dictionary<string, IComponentSpec>(original._components);
        _afterInitHooks = [..original._afterInitHooks];
    }

    public Template AddDataAccess<TDataAccess>(string tenantName)
        where TDataAccess : class
    {
        _dataAccess.Add(typeof(TDataAccess), tenantName);
        return this;
    }

    public Template AddTenant<TTenant, TTenantSpec>(string tenantName, string componentName, Func<TTenantSpec, TTenantSpec>? config = null)
        where TTenantSpec : TenantSpec<TTenant>, new()
    {
        var spec = new TTenantSpec
        {
            ComponentName = componentName
        };
        spec = config?.Invoke(spec) ?? spec;
        _tenants.Add(tenantName, spec);
        return this;
    }

    public Template AddComponent<TComponent, TComponentSpec>(string componentName, Func<TComponentSpec, TComponentSpec>? config = null)
        where TComponentSpec : ComponentSpec<TComponent>, new()
    {
        var spec = new TComponentSpec();
        spec = config?.Invoke(spec) ?? spec;
        _components.Add(componentName, spec);
        return this;
    }

    /// <summary>
    /// Add a hook that is called after tenants are created, but before the test method is being executed.
    /// </summary>
    /// <param name="hook">A function that receives all created for a test.</param>
    public Template AddAfterInit(Func<IReadOnlyCollection<Tenant>, CancellationToken, Task> hook)
    {
        _afterInitHooks.Add(hook);
        return this;
    }

    internal async Task ApplyAfterInitAsync(IReadOnlyCollection<Tenant> tenants)
    {
        foreach (var afterInitHook in _afterInitHooks)
            await afterInitHook(tenants, default);
    }
}
