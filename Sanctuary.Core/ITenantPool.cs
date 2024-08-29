using JetBrains.Annotations;
using System.Threading.Tasks;

namespace Sanctuary;

[PublicAPI]
public interface ITenantFactory
{
    Task<object> AddTenantAsync(string tenantName, object? dataSource);

    Task RemoveTenantAsync(object tenant);
}