using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sanctuary;

internal class TenantsFactory : ITenantsFactory
{
    private readonly Dictionary<string, Template> _templates;
    private readonly IReadOnlyDictionary<string, ITenantFactory> _tenantFactories;

    /// <summary>
    /// Key: type of component. Value: <see cref="IComponentPool{TComponent}">pool</see> of the component.
    /// </summary>
    private readonly IReadOnlyDictionary<Type, object> _componentPools;

    internal TenantsFactory(
        Dictionary<string, Template> templates, 
        IReadOnlyDictionary<string, ITenantFactory> tenantFactories,
        IReadOnlyDictionary<Type, object> componentPools)
    {
        _templates = templates;
        _tenantFactories = tenantFactories;
        _componentPools = componentPools;
    }

    public async Task<IReadOnlyCollection<TenantInfo>> AddTenantsAsync(string templateName)
    {
        var tenants = new List<TenantInfo>();

        var template = _templates[templateName];
        var usedComponents = GetComponents(template);
        var allComponents = new Dictionary<string, object>();
        foreach (var (componentType, componentSpecs) in usedComponents)
        {
            var componentPool = _componentPools[componentType];
            var interfaceType = componentPool.GetType().GetInterface(typeof(IComponentPool<>).Name);
            var getComponentMethod = interfaceType?.GetMethod(nameof(IComponentPool<object>.AcquireComponents));
            if (getComponentMethod is null)
                throw new UnreachableException($"Type '{componentPool.GetType()}' is not a {typeof(IComponentPool<>)}.");

            // TODO: Shouldn't use IDictionary, but IReadOnlyDictionary<string, TComponent>
            IDictionary acquiredComponents = (IDictionary)getComponentMethod.Invoke(componentPool, [componentSpecs]);

            foreach (DictionaryEntry entry in acquiredComponents)
            {
                var componentName = (string)entry.Key;
                var component = entry.Value;
                allComponents.Add(componentName, component);
            }

            var tenantDataAccesses = template._dataAccess
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

            foreach (var (tenantName, tenantConfig) in template._tenants)
            {
                if (!_tenantFactories.TryGetValue(tenantConfig.ComponentName, out var factory))
                    throw new InvalidOperationException("Missing pool");

                var component = acquiredComponents[tenantConfig.ComponentName];
                var tenant = await factory.AddTenantAsync(component, tenantName, tenantConfig.DataSource);
                var tenantInfo = new TenantInfo(
                    tenant,
                    tenantName,
                    tenantConfig.ComponentName,
                    component,
                    tenantDataAccesses[tenantName]);
                tenants.Add(tenantInfo);
            }
        }

        return tenants;
    }

    public async Task RemoveTenantsAsync(IEnumerable<TenantInfo> tenants)
    {
        foreach (var tenantInfo in tenants)
        {
            var tenantFactory = _tenantFactories[tenantInfo.ComponentName];
            await tenantFactory.RemoveTenantAsync(tenantInfo.Component, tenantInfo.Instance);
        }
    }

    private static Dictionary<Type, Dictionary<string, ComponentSpec>> GetComponents(Template template)
    {
        var result = new Dictionary<Type, Dictionary<string, ComponentSpec>>();
        var usedComponents = template._components.ToLookup(
            x => x.Value.ComponentType,
            x => (ComponentName: x.Key, Spec: x.Value));

        foreach (var oneTypeComponents in usedComponents)
        {
            Type componentType = oneTypeComponents.Key;
            Dictionary<string, ComponentSpec> componentSpecs =
                oneTypeComponents.ToDictionary(x => x.ComponentName, x => x.Spec);

            result.Add(componentType, componentSpecs);
        }

        return result;
    }
}