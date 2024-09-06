using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IComponentPool = object;
using ITenantFactory = object;

namespace DataIsland;

internal class Materializer : IMaterializer
{
    private readonly Dictionary<string, Template> _templates;

    /// <summary>
    /// Key: component name. Value: <see cref="ITenantFactory{TTenant,TComponent,TDataSource}"/>.
    /// </summary>
    private readonly IReadOnlyDictionary<string, ITenantFactory> _tenantFactories;

    /// <summary>
    /// Key: type of component. Value: <see cref="IComponentPool{TComponent,TComponentSpec}">
    /// pool</see> of the component. Each type has a different spec type in the value, so we
    /// can't use strong types.
    /// </summary>
    private readonly IReadOnlyDictionary<Type, IComponentPool> _componentPools;

    internal Materializer(
        Dictionary<string, Template> templates,
        IReadOnlyDictionary<string, ITenantFactory> tenantFactories,
        IReadOnlyDictionary<Type, IComponentPool> componentPools)
    {
        _templates = templates;
        _tenantFactories = tenantFactories;
        _componentPools = componentPools;
    }

    public async Task<IReadOnlyCollection<Tenant>> MaterializeTenantsAsync(string templateName)
    {
        var tenants = new List<Tenant>();

        var template = _templates[templateName];
        var usedComponents = GetComponents(template);
        foreach (var (componentType, componentSpecs) in usedComponents)
        {
            var componentPool = _componentPools[componentType];
            var acquiredComponents = await componentPool.AcquireComponentsAsync(componentSpecs);

            var tenantDataAccesses = template._dataAccess
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

            foreach (var (tenantName, tenantSpec) in template._tenants)
            {
                var factory = _tenantFactories[tenantSpec.ComponentName];
                var component = acquiredComponents[tenantSpec.ComponentName]!;

                var tenant = await factory.AddTenantAsync(component, tenantSpec);
                var tenantInfo = new Tenant(
                    tenant,
                    tenantName,
                    tenantSpec.ComponentName,
                    component,
                    tenantDataAccesses[tenantName]);
                tenants.Add(tenantInfo);
            }
        }

        return tenants;
    }

    public async Task DematerializeTenantsAsync(IEnumerable<Tenant> tenants)
    {
        foreach (var tenantInfo in tenants)
        {
            var tenantFactory = _tenantFactories[tenantInfo.ComponentName];
            await tenantFactory.RemoveTenantAsync(tenantInfo.Component, tenantInfo.Instance);
        }
    }

    private static Dictionary<Type, object> GetComponents(Template template)
    {
        // The dictionary value is Dictionary<string, {Some}Spec>
        // but we can't do that, because value is invariant. The
        // value would have to be covariant, but that would cause
        // serious problems (potential runtime exception).
        var result = new Dictionary<Type, object>();
        var usedComponents = template._components.ToLookup(
            x =>
            (
                ComponentSpecType: x.Value.GetType(),
                x.Value.ComponentType
            ),
            x => (ComponentName: x.Key, Spec: x.Value));

        foreach (var oneTypeComponents in usedComponents)
        {
            Type componentType = oneTypeComponents.Key.ComponentType;
            Type componentSpecType = oneTypeComponents.Key.ComponentSpecType;

            var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(string), componentSpecType);
            var oneTypeSpecs = (IDictionary?)Activator.CreateInstance(dictionaryType)!;
            foreach (var (componentName, componentSpec) in oneTypeComponents)
                oneTypeSpecs.Add(componentName, componentSpec);

            result.Add(componentType, oneTypeSpecs);
        }

        return result;
    }
}
