using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanctuary;

/// <summary>
/// A factory that creates and released all tenants of a profile. The factory is used
/// </summary>
public interface ITenantsFactory
{
    /// <summary>
    /// Create all tenants defined in a profile.
    /// </summary>
    /// <param name="profileName">Name of the profile.</param>
    Task<IReadOnlyCollection<TenantInfo>> AddTenantsAsync(string profileName);

    Task RemoveTenantsAsync(IEnumerable<TenantInfo> tenants);
}