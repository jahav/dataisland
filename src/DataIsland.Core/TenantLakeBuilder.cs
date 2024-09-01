using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class TenantLakeBuilder
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
    /// a component, it will use a logical name (<see cref="componentName"/>) that be used to
    /// select this pool.
    /// </summary>
    /// <typeparam name="TComponent">Type of component the pool will be providing.</typeparam>
    /// <typeparam name="TComponentSpec">The component specification of a <typeparamref name="TComponent"/>.</typeparam>
    /// <typeparam name="TTenant">Tenant that can be created in the component.</typeparam>
    /// <typeparam name="TTenantSpec">The tenant specification of a <typeparamref name="TTenant"/>.</typeparam>
    /// <param name="componentName">Logical name of a component that will be used in <see cref="Template"/>.</param>
    /// <param name="componentPool">Instance of a pool that will be providing the components.</param>
    /// <param name="tenantFactory">Factory that is going to create tenants on components from <paramref name="componentPool"/>.</param>
    /// <exception cref="ArgumentException">Component pool for the <typeparamref name="TComponent"/> has already been registered.</exception>
    /// <exception cref="ArgumentException"><paramref name="componentName"/> has already been used.</exception>
    public TenantLakeBuilder AddComponentPool<TComponent, TComponentSpec, TTenant, TTenantSpec>(
        string componentName,
        IComponentPool<TComponent, TComponentSpec> componentPool,
        ITenantFactory<TTenant, TComponent, TTenantSpec> tenantFactory)
        where TComponentSpec : ComponentSpec<TComponent>
        where TTenantSpec : TenantSpec<TTenant>
    {
        var addedPool = _componentPools.TryAdd(typeof(TComponent), componentPool);
        if (!addedPool)
            throw new ArgumentException($"Component pool for {typeof(TComponent)} is already registered.");

        var addedFactory = _tenantFactories.TryAdd(componentName, tenantFactory);
        if (!addedFactory)
            throw new ArgumentException($"Component name '{componentName}' is already registered.");

        return this;
    }

    public TenantLakeBuilder AddTemplate(string templateName, Action<Template> configure)
    {
        var template = new Template();
        configure(template);
        _templates.Add(templateName, template);
        return this;
    }

    public TenantLakeBuilder AddPatcher<TDataAccess>(IDependencyPatcher<TDataAccess> patcher)
    {
        _patchers.Add(typeof(TDataAccess), patcher);
        return this;
    }

    public ITenantLake Build(ITestContext testContext)
    {
        // TODO: Validate everything
        var patchersCopy = new Dictionary<Type, object>(_patchers);
        var tenantFactoriesCopy = new Dictionary<string, object>(_tenantFactories);
        var templatesCopy = _templates.ToDictionary(x => x.Key, x => new Template(x.Value));
        var componentPoolsCopy = _componentPools.ToDictionary(x => x.Key, x => x.Value);
        var materializer = new Materializer(templatesCopy, tenantFactoriesCopy, componentPoolsCopy);
        return new TenantLake(materializer, testContext, patchersCopy);
    }
}

// TODO: Remove once Polyfill contains method TryAdd
#if NETSTANDARD2_0
internal static class DictionaryExtensions
{
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}
#endif
