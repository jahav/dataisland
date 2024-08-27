using System.Threading.Tasks;

namespace Sanctuary;

public interface ITenantPool
{
    Task<object> AddTenantAsync(string tenantName, object dataSource);

    Task RemoveTenantAsync(object tenant);
}