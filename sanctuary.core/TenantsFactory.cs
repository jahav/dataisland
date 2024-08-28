using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanctuary;

internal class TenantsFactory : ITenantsFactory
{
    private readonly Dictionary<string, DataAccessProfile> _profiles;
    private readonly IReadOnlyDictionary<string, ITenantPool> _pools;

    internal TenantsFactory(Dictionary<string, DataAccessProfile> profiles, IReadOnlyDictionary<string, ITenantPool> pools)
    {
        _profiles = profiles;
        _pools = pools;
    }

    public async Task<IReadOnlyCollection<TenantInfo>> AddTenantsAsync(string profileName)
    {
        var profile = _profiles[profileName];
        var tenants = new List<TenantInfo>();
        var tenantDataAccesses = profile._dataAccess
            .GroupBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

        var reachableTenants = new HashSet<string>(profile._dataAccess.Values);
        foreach (var (tenantName, tenantConfig) in profile._tenants)
        {
            // If tenant is not used by any data access, it's useless to create it.
            if (!reachableTenants.Contains(tenantName))
                continue;

            if (!_pools.TryGetValue(tenantConfig.ComponentName, out var pool))
                throw new InvalidOperationException("Missing pool");

            var tenant = await pool.AddTenantAsync(tenantName, tenantConfig.DataSource);
            var tenantInfo = new TenantInfo(
                tenant,
                tenantName,
                tenantConfig.ComponentName,
                tenantDataAccesses[tenantName]);
            tenants.Add(tenantInfo);
        }

        return tenants;
    }

    public async Task RemoveTenantsAsync(IEnumerable<TenantInfo> tenants)
    {
        foreach (var tenantInfo in tenants)
        {
            var pool = _pools[tenantInfo.ComponentName];
            await pool.RemoveTenantAsync(tenantInfo.Instance);
        }
    }
}