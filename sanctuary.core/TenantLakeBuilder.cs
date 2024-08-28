using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class TenantLakeBuilder
{
    private readonly Dictionary<string, ITenantPool> _componentPools = new();
    private readonly Dictionary<string, DataAccessProfile> _profiles = new();

    public TenantLakeBuilder AddComponentPool<TTenant, TDataSource>(string componentName, ITenantPool<TTenant, TDataSource> pool)
    {
        _componentPools.Add(componentName, pool);
        return this;
    }

    public TenantLakeBuilder AddProfile(string profileName, Action<DataAccessProfile> configure)
    {
        var profile = new DataAccessProfile();
        configure(profile);

        _profiles.Add(profileName, profile);
        return this;
    }

    public ITenantLake Build(ITestContext testContext)
    {
        var componentPoolsCopy = new Dictionary<string, ITenantPool>(_componentPools);
        var profilesCopy = _profiles.ToDictionary(x => x.Key, x => new DataAccessProfile(x.Value));
        var factory = new TenantsFactory(profilesCopy, componentPoolsCopy);
        return new TenantLake(factory, testContext);
    }
}