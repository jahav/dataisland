using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanctuary;

public class TenantsFactory(SanctuaryConfig _config, IReadOnlyDictionary<string, ITenantPool> _pools) : ITenantsFactory
{
    public async Task<Dictionary<Type, object>> AddTenantsAsync(string profileName)
    {
        var profile = _config.GetProfile(profileName);
        var tenants = new Dictionary<string, object>();

        var reachableTenants = new HashSet<string>(profile._dataAccess.Values);
        foreach (var (tenantName, tenantConfig) in profile._tenants)
        {
            // If tenant is not used by any data access, it's useless to create it.
            if (!reachableTenants.Contains(tenantName))
                continue;

            if (!_pools.TryGetValue(tenantConfig.ComponentName, out var pool))
                throw new InvalidOperationException("Missing pool");

            var tenant = await pool.AddTenantAsync(tenantName, tenantConfig.DataSource);
            tenants.Add(tenantName, tenant);
        }

        var dataAccessMap = new Dictionary<Type, object>(profile._dataAccess.Count);
        foreach (var (dataAccessType, tenantName) in profile._dataAccess)
            dataAccessMap.Add(dataAccessType, tenants[tenantName]);

        return dataAccessMap;
    }

    public Task RemoveTenantsAsync(Dictionary<Type, object> tenants)
    {
        throw new NotImplementedException();
    }
}