using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanctuary;

/// <summary>
/// A factory that creates and releases all tenants of a logical view.
/// </summary>
public interface ITenantsFactory
{
    /// <summary>
    /// Create all tenants defined in a logical view.
    /// </summary>
    /// <param name="logicalViewName">Name of the logical view.</param>
    Task<IReadOnlyCollection<TenantInfo>> AddTenantsAsync(string logicalViewName);

    Task RemoveTenantsAsync(IEnumerable<TenantInfo> tenants);
}