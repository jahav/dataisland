using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class TenantLakeBuilder
{
    /// <summary>
    /// Key: component name. Value: tenant factory.
    /// </summary>
    private readonly Dictionary<string, ITenantFactory> _tenantFactories = new();

    /// <summary>
    /// Key: type of component. Value: IComponentPool.
    /// </summary>
    private readonly Dictionary<Type, object> _componentPools = new();
    private readonly Dictionary<string, LogicalView> _logicalViews = new();
    private readonly Dictionary<Type, object> _patchers = new();

    public TenantLakeBuilder AddComponent<TTenant, TComponent, TDataSource>(
        string componentName, 
        IComponentPool<TComponent> componentPool, 
        ITenantFactory<TTenant, TComponent, TDataSource> factory)
    {
        _componentPools.Add(typeof(TComponent), componentPool);
        _tenantFactories.Add(componentName, factory);
        return this;
    }

    public TenantLakeBuilder AddLogicalView(string logicalViewName, Action<LogicalView> configure)
    {
        var logicalView = new LogicalView();
        configure(logicalView);

        _logicalViews.Add(logicalViewName, logicalView);
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
        var patchersCopy = _patchers.Values.ToList();
        var tenantFactoriesCopy = new Dictionary<string, ITenantFactory>(_tenantFactories);
        var logicalViewsCopy = _logicalViews.ToDictionary(x => x.Key, x => new LogicalView(x.Value));
        var componentPoolsCopy = _componentPools.ToDictionary(x => x.Key, x => x.Value);
        var factory = new TenantsFactory(logicalViewsCopy, tenantFactoriesCopy, componentPoolsCopy);
        return new TenantLake(factory, testContext, patchersCopy);
    }
}