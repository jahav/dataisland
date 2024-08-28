using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class TenantLakeBuilder : ITenantLake
{
    private readonly Dictionary<string, ITenantPool> _componentPools = new();
    private readonly Dictionary<string, DataAccessProfile> _profiles = new();
    private readonly ITenantsFactory _factory;

    public TenantLakeBuilder()
    {
        _factory = new TenantsFactory(this, _componentPools);
    }

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
    internal DataAccessProfile GetProfile(string testContextProfile)
    {
        return _profiles[testContextProfile];
    }

    ////////////////////////////////////////
    public ITenantLake Build(ITestContext testContext)
    {
        // TODO: Make an immutable object. This can be modified by calling additional methods that modify the builder.
        _testContext = testContext;
        return this;
    }

    private ITestContext _testContext;

    ITenantsFactory ITenantLake.Factory => _factory;

    ITestContext ITenantLake.TestContext => _testContext;
}