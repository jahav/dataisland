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
    /// <returns>
    ///     A dictionary of created tenants mapped by a type of data access.
    ///     Note that multiple data access keys can point to same tenant.
    /// </returns>
    Task<Dictionary<Type, Tenant>> AddTenantsAsync(string profileName);

    Task RemoveTenantsAsync(Dictionary<Type, Tenant> tenants);
}