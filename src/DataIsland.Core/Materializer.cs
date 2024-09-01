using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DataIsland;

internal class Materializer : IMaterializer
{
    private readonly Dictionary<string, Template> _templates;

    /// <summary>
    /// Key: component name. Value: <see cref="ITenantFactory{TTenant,TComponent,TDataSource}"/>.
    /// </summary>
    private readonly IReadOnlyDictionary<string, object> _tenantFactories;

    /// <summary>
    /// Key: type of component. Value: <see cref="IComponentPool{TComponent,TComponentSpec}">
    /// pool</see> of the component. Each type has a different spec type in the value, so we
    /// can't use strong types.
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

    public async Task<IReadOnlyCollection<Tenant>> MaterializeTenantsAsync(string templateName)
    {
        var tenants = new List<Tenant>();

        var template = _templates[templateName];
        var usedComponents = GetComponents(template);
        foreach (var (componentType, componentSpecs) in usedComponents)
        {
            var componentPool = _componentPools[componentType];
            var acquiredComponents = await CallAcquiredComponentsAsync(componentPool, componentSpecs);

            var tenantDataAccesses = template._dataAccess
                .GroupBy(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

            foreach (var (tenantName, tenantSpec) in template._tenants)
            {
                if (!_tenantFactories.TryGetValue(tenantSpec.ComponentName, out var factory))
                    throw new InvalidOperationException("Missing pool");

                var component = acquiredComponents[tenantSpec.ComponentName]!;

                // Dynamic call of await factory.AddTenantAsync(component, tenantSpec);
                var tenant = await CallAddTenantAsync(factory, component, tenantSpec);
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
            await CallRemoveTenantAsync(tenantFactory, tenantInfo.Component, tenantInfo.Instance);
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

    private static async Task<IDictionary> CallAcquiredComponentsAsync(object componentPool, object componentSpecs)
    {
        var componentPoolInterface = componentPool.GetType().GetInterface(typeof(IComponentPool<,>).Name);
        Debug.Assert(componentPoolInterface is not null);
        var getComponentMethod = componentPoolInterface.GetMethod("AcquireComponentsAsync");
        Debug.Assert(getComponentMethod is not null);
        var task = (Task?)getComponentMethod.Invoke(componentPool, [componentSpecs]);
        Debug.Assert(task is not null);
        await task;
        var resultProperty = task.GetType().GetProperty("Result");
        Debug.Assert(resultProperty is not null);

        // TODO: Shouldn't use IDictionary, but IReadOnlyDictionary<string, TComponent>
        var acquiredComponents = (IDictionary?)resultProperty.GetValue(task);
        Debug.Assert(acquiredComponents is not null);
        return acquiredComponents;
    }

    private static async Task<object> CallAddTenantAsync(object tenantFactory, object component, object tenantSpec)
    {
        // Actually return Task<TTenant>
        var taskWithResult = await InvokeTenantFactoryMethod(tenantFactory, "AddTenantAsync", [component, tenantSpec]);
        var resultProperty = taskWithResult.GetType().GetProperty("Result");
        Debug.Assert(resultProperty is not null);
        var tenant = resultProperty.GetValue(taskWithResult);
        Debug.Assert(tenant is not null);
        return tenant;
    }

    private static async Task CallRemoveTenantAsync(object tenantFactory, object component, object tenant)
    {
        await InvokeTenantFactoryMethod(tenantFactory, "RemoveTenantAsync", [component, tenant]);
    }

    private static async ValueTask<Task> InvokeTenantFactoryMethod(object tenantFactory, string methodName, object[] args)
    {
        var tenantFactoryInterface = tenantFactory.GetType().GetInterface(typeof(ITenantFactory<,,>).Name);
        Debug.Assert(tenantFactoryInterface is not null);
        var method = tenantFactoryInterface.GetMethod(methodName);
        Debug.Assert(method is not null);
        var task = (Task?)method.Invoke(tenantFactory, args);
        Debug.Assert(task is not null);
        await task;
        return task;
    }
}
