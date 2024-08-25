using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class SanctuaryConfig
{
    private readonly Dictionary<string, ITenantPool> _componentPools = new();
    private readonly Dictionary<string, DataAccessProfile> _profiles = new();

    public SanctuaryConfig()
    {
    }

    public SanctuaryConfig RegisterComponentPool<TTenant, TDataSource>(string componentName, ITenantPool<TTenant, TDataSource> pool)
    {
        _componentPools.Add(componentName, pool);
        return this;
    }

    public SanctuaryConfig AddProfile(string profileName, Action<DataAccessProfile> configure)
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
}