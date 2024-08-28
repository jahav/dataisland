using JetBrains.Annotations;
using System.Threading.Tasks;

namespace Sanctuary;

[PublicAPI]
public interface ITenantPool
{
    Task<object> AddTenantAsync(string tenantName, object? dataSource);

    Task RemoveTenantAsync(object tenant);
}