using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class TenantLakeBuilder
{
    private readonly Dictionary<string, ITenantFactory> _tenantFactories = new();
    private readonly Dictionary<string, LogicalView> _logicalViews = new();
    private readonly Dictionary<Type, object> _patchers = new();

    public TenantLakeBuilder AddComponentFactory<TTenant, TDataSource>(string componentName, ITenantFactory<TTenant, TDataSource> factory)
    {
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
        var factory = new TenantsFactory(logicalViewsCopy, tenantFactoriesCopy);
        return new TenantLake(factory, testContext, patchersCopy);
    }
}