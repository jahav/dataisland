using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace DataIsland;

[PublicAPI]
public class DataIslandBuilder
{
    /// <summary>
    /// Key: component name. Value: <see cref="ITenantFactory{TTenant,TComponent,TDataSource}"/>.
    /// </summary>
    private readonly Dictionary<string, object> _tenantFactories = new();

    /// <summary>
    /// Key: type of component. Value: IComponentPool.
    /// </summary>
    private readonly Dictionary<Type, object> _componentPools = new();
    private readonly Dictionary<string, Template> _templates = new();
    private readonly Dictionary<Type, object> _patchers = new();

    /// <summary>
    /// Register a component pool that will be providing components when <see cref="Template"/>
    /// is going to be instantiated. When a <see cref="Template"/> says it wants to use
    /// a component, it will use a logical name (<see cref="componentPoolName"/>) that be used to
    /// select this pool.
    /// </summary>
    /// <typeparam name="TComponent">Type of component the pool will be providing.</typeparam>
    /// <typeparam name="TComponentSpec">The component specification of a <typeparamref name="TComponent"/>.</typeparam>
    /// <typeparam name="TTenant">Tenant that can be created in the component.</typeparam>
    /// <typeparam name="TTenantSpec">The tenant specification of a <typeparamref name="TTenant"/>.</typeparam>
    /// <param name="componentPoolName">Name of the pool. The name is used in <see cref="Template"/> configuration.</param>
    /// <param name="componentPool">Instance of a pool that will be providing the components.</param>
    /// <param name="tenantFactory">Factory that is going to create tenants on components from <paramref name="componentPool"/>.</param>
    /// <exception cref="ArgumentException">Component pool for the <typeparamref name="TComponent"/> has already been registered.</exception>
    /// <exception cref="ArgumentException"><paramref name="componentPoolName"/> has already been used.</exception>
    public DataIslandBuilder AddComponentPool<TComponent, TComponentSpec, TTenant, TTenantSpec>(
        string componentPoolName,
        IComponentPool<TComponent, TComponentSpec> componentPool,
        ITenantFactory<TTenant, TComponent, TTenantSpec> tenantFactory)
        where TComponentSpec : ComponentSpec<TComponent>
        where TTenantSpec : TenantSpec<TTenant>
    {
        var addedPool = _componentPools.TryAdd(typeof(TComponent), componentPool);
        if (!addedPool)
            throw new ArgumentException($"Component pool for {typeof(TComponent)} is already registered.");

        var addedFactory = _tenantFactories.TryAdd(componentPoolName, tenantFactory);
        if (!addedFactory)
            throw new ArgumentException($"Component name '{componentPoolName}' is already registered.");

        return this;
    }

    public DataIslandBuilder AddTemplate(string templateName, Action<Template> configure)
    {
        var template = new Template();
        configure(template);
        _templates.Add(templateName, template);
        return this;
    }

    public DataIslandBuilder AddPatcher<TDataAccess>(IDependencyPatcher<TDataAccess> patcher)
    {
        _patchers.Add(typeof(TDataAccess), patcher);
        return this;
    }

    public IDataIsland Build(ITestContext testContext)
    {
        // TODO: Validate everything
        foreach (var (_, template) in _templates)
        {
            var availablePools = _tenantFactories.Keys;
            foreach (var (_, tenantSpec) in template._tenants)
            {
                // Tenant must refer to existing pool.
                if (!availablePools.Contains(tenantSpec.ComponentName))
                {
                    var availablePoolNames = string.Join(",", _tenantFactories.Keys.Select(x => $"'{x}'"));
                    throw new InvalidOperationException($"Unable to find pool '{tenantSpec.ComponentName}'. Available pools: {availablePoolNames}.");
                }
            }
        }

        var patchersCopy = new Dictionary<Type, object>(_patchers);
        var tenantFactoriesCopy = new Dictionary<string, object>(_tenantFactories);
        var templatesCopy = _templates.ToDictionary(x => x.Key, x => new Template(x.Value));
        var componentPoolsCopy = _componentPools.ToDictionary(x => x.Key, x => x.Value);
        var materializer = new Materializer(templatesCopy, tenantFactoriesCopy, componentPoolsCopy);
        return new DataIsland(materializer, testContext, patchersCopy);
    }
}
