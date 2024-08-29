using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanctuary;

internal class TenantsFactory : ITenantsFactory
{
    private readonly Dictionary<string, LogicalView> _logicalViews;
    private readonly IReadOnlyDictionary<string, ITenantFactory> _pools;

    internal TenantsFactory(Dictionary<string, LogicalView> logicalViews, IReadOnlyDictionary<string, ITenantFactory> pools)
    {
        _logicalViews = logicalViews;
        _pools = pools;
    }

    public async Task<IReadOnlyCollection<TenantInfo>> AddTenantsAsync(string logicalViewName)
    {
        var logicalView = _logicalViews[logicalViewName];
        var tenants = new List<TenantInfo>();
        var tenantDataAccesses = logicalView._dataAccess
            .GroupBy(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Select(y => y.Key).ToHashSet());

        var reachableTenants = new HashSet<string>(logicalView._dataAccess.Values);
        foreach (var (tenantName, tenantConfig) in logicalView._tenants)
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