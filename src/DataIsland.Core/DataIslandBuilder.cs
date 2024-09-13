using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using IComponentPool = object;
using ITenantFactory = object;
using IDependencyPatcher = object;

namespace DataIsland;

[PublicAPI]
public class DataIslandBuilder
{
    /// <summary>
    /// Key: component name. Value: <see cref="ITenantFactory{TTenant,TComponent,TDataSource}"/>.
    /// </summary>
    private readonly Dictionary<string, ITenantFactory> _tenantFactories = [];

    /// <summary>
    /// Key: type of component. Value: IComponentPool.
    /// </summary>
    private readonly Dictionary<Type, IComponentPool> _componentPools = [];
    private readonly Dictionary<string, Template> _templates = [];
    private readonly Dictionary<Type, IDependencyPatcher> _patchers = [];

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

    public IDataIsland Build()
    {
        foreach (var (templateName, template) in _templates)
        {
            var availablePools = _tenantFactories.Keys;
            var specifiedComponents = template._components.Keys;
            var referencedComponents = template._tenants.Values.Select(x => x.ComponentName).ToList();
            foreach (var (_, tenantSpec) in template._tenants)
            {
                var tenantComponentName = tenantSpec.ComponentName;

                // All tenant must refer to existing pool.
                if (!availablePools.Contains(tenantComponentName))
                {
                    var availablePoolNames = string.Join(",", _tenantFactories.Keys.Select(x => $"'{x}'"));
                    throw new InvalidOperationException($"Unable to find pool '{tenantSpec.ComponentName}'. Available pools: {availablePoolNames}. Use method DataIslandBuilder.AddComponentPool(poolName) to add a pool.");
                }

                // All tenants must refer to specified component.
                if (!specifiedComponents.Contains(tenantComponentName))
                {
                    throw new InvalidOperationException($"Template '{templateName}' didn't specify component '{tenantComponentName}'. Specify the component in the template by template.AddComponent(\"{tenantComponentName}\") for the template. Method has optional second parameter that contains required properties of component resolved from the pool.");
                }
            }

            // No component can be orphaned
            var orphanedComponents = specifiedComponents.Except(referencedComponents).ToList();
            if (orphanedComponents.Count > 0)
            {
                var orphanedComponentNames = string.Join("','", orphanedComponents);
                throw new InvalidOperationException($"Template '{templateName}' specified component '{orphanedComponentNames}', but that component wasn't used. Remove it.");
            }

            // No tenant can be orphaned.
            var referencedTenants = template._dataAccess.Values;
            var specifiedTenants = template._tenants.Keys;
            var orphanedTenants = specifiedTenants.Except(referencedTenants);
            var orphanedTenantsString = string.Join("','", orphanedTenants);
            if (orphanedTenantsString.Length > 0)
            {
                throw new InvalidOperationException($"Template '{templateName}' specifies unused tenants '{orphanedTenantsString}'. Remove them.");
            }

            // All referenced tenants must be present.
            var missingTenants = referencedTenants.Except(specifiedTenants);
            var missingTenantsString = string.Join("','", missingTenants);
            if (missingTenantsString.Length > 0)
            {
                throw new InvalidOperationException($"Template '{templateName}' didn't specify component '{missingTenantsString}'. Specify the tenant in the template by template.AddTenant(tenantName). Method has optional second parameter that contains required properties of tenant that will be created.");
            }

            if (template._dataAccess.Count == 0)
            {
                throw new InvalidOperationException($"Template '{templateName}' doesn't specify any data access. Add it by using tenant.AddDataAccess method.");
            }
        }

        var patchersCopy = new Dictionary<Type, IDependencyPatcher>(_patchers);
        var tenantFactoriesCopy = new Dictionary<string, ITenantFactory>(_tenantFactories);
        var templatesCopy = _templates.ToDictionary(x => x.Key, x => new Template(x.Value));
        var componentPoolsCopy = _componentPools.ToDictionary(x => x.Key, x => x.Value);
        var materializer = new Materializer(templatesCopy, tenantFactoriesCopy, componentPoolsCopy);
        return new DataIsland(materializer, patchersCopy);
    }
}
