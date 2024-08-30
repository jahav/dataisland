using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sanctuary;

internal class Materializer : IMaterializer
{
    private readonly Dictionary<string, Template> _templates;

    /// <summary>
    /// Key: component name. Value: <see cref="ITenantFactory{TTenant,TComponent,TDataSource}"/>.
    /// </summary>
    private readonly IReadOnlyDictionary<string, object> _tenantFactories;

    /// <summary>
    /// Key: type of component. Value: <see cref="IComponentPool{TComponent}">pool</see> of the component.
    /// </summary>
    private readonly IReadOnlyDictionary<Type, object> _componentPools;

    internal Materializer(
        Dictionary<string, Template> templates, 
        IReadOnlyDictionary<string, object> tenantFactories,
        IReadOnlyDictionary<Type, object> componentPools)
    {
        _templates = templates;
        _tenantFactories = tenantFactories;
        _componentPools = componentPools;
    }

    public async Task<IReadOnlyCollection<TenantInfo>> MaterializeTenantsAsync(string templateName)
    {
        var tenants = new List<TenantInfo>();

        var template = _templates[templateName];
        var usedComponents = GetComponents(template);
        foreach (var (componentType, componentSpecs) in usedComponents)
        {
            var componentPool = _componentPools[componentType];
            var interfaceType = componentPool.GetType().GetInterface(typeof(IComponentPool<>).Name);
            var getComponentMethod = interfaceType?.GetMethod(nameof(IComponentPool<object>.AcquireComponents));
            if (getComponentMethod is null)
                throw new UnreachableException($"Type '{componentPool.GetType()}' is not a {typeof(IComponentPool<>)}.");

            // TODO: Shouldn't use IDictionary, but IReadOnlyDictionary<string, TComponent>
            IDictionary acquiredComponents = (IDictionary)getComponentMethod.Invoke(componentPool, [componentSpecs]);

            var tenantDataAccesses = template._dataAccess
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

            foreach (var (tenantName, tenantConfig) in template._tenants)
            {
                if (!_tenantFactories.TryGetValue(tenantConfig.ComponentName, out var factory))
                    throw new InvalidOperationException("Missing pool");

                var component = acquiredComponents[tenantConfig.ComponentName];

                // Dynamic call of await factory.AddTenantAsync(component, tenantName, tenantConfig.DataSource);
                var tenant = await CallAddTenantAsync(factory, component, tenantName, tenantConfig.DataSource);
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

    public async Task DematerializeTenantsAsync(IEnumerable<TenantInfo> tenants)
    {
        foreach (var tenantInfo in tenants)
        {
            var tenantFactory = _tenantFactories[tenantInfo.ComponentName];
            await CallRemoveTenantAsync(tenantFactory, tenantInfo.Component, tenantInfo.Instance);
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

    private async Task<object> CallAddTenantAsync(object tenantFactory, object component, string tenantName, object? dataSource)
    {
        var interfaceType = tenantFactory.GetType().GetInterface(typeof(ITenantFactory<,,>).Name);
        var method = interfaceType?.GetMethod("AddTenantAsync");
        if (method is null)
            throw new UnreachableException();

        var taskWithResult = (Task)method.Invoke(tenantFactory, [component, tenantName, dataSource]);
        await taskWithResult;
        var resultProperty = taskWithResult.GetType().GetProperty("Result");
        if (resultProperty is null)
            throw new UnreachableException();

        return resultProperty.GetValue(taskWithResult);
    }

    private async Task CallRemoveTenantAsync(object tenantFactory, object component, object tenant)
    {
        var interfaceType = tenantFactory.GetType().GetInterface(typeof(ITenantFactory<,,>).Name);
        var method = interfaceType?.GetMethod("RemoveTenantAsync");
        if (method is null)
            throw new UnreachableException();

        var task = (Task)method.Invoke(tenantFactory, [component, tenant]);
        await task;
    }
}