using JetBrains.Annotations;
using System.Threading.Tasks;

namespace Sanctuary;

[PublicAPI]
public interface ITenantFactory
{
    Task<object> AddTenantAsync(object component, string tenantName, object? dataSource);

    Task RemoveTenantAsync(object component, object tenant);
}