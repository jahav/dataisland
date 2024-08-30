using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanctuary;

/// <summary>
/// A factory that creates and releases all tenants of a template.
/// </summary>
public interface ITenantsFactory
{
    /// <summary>
    /// Create all tenants defined in a template.
    /// </summary>
    /// <param name="templateName">Name of the template.</param>
    Task<IReadOnlyCollection<TenantInfo>> AddTenantsAsync(string templateName);

    Task RemoveTenantsAsync(IEnumerable<TenantInfo> tenants);
}