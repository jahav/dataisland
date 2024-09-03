using JetBrains.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataIsland;

/// <summary>
/// A factory that materializes a <see cref="Template"/>. It's called before/after a test to
/// create/delete tenants.
/// </summary>
[PublicAPI]
public interface IMaterializer
{
    /// <summary>
    /// Create all tenants defined in a template on matching components from pools.
    /// </summary>
    /// <param name="templateName">Name of the template.</param>
    /// <returns>A collection of all tenants from <see cref="Template"/></returns>
    /// <exception cref="KeyNotFoundException">Template <paramref name="templateName"/> is not found.</exception>
    Task<IReadOnlyCollection<Tenant>> MaterializeTenantsAsync(string templateName);

    /// <summary>
    /// Delete passed tenants. The tenants are from <see cref="MaterializeTenantsAsync"/>.
    /// </summary>
    /// <param name="tenants">Tenants to dematerialize.</param>
    Task DematerializeTenantsAsync(IEnumerable<Tenant> tenants);
}
