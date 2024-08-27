using System.Threading.Tasks;

namespace Sanctuary;

public interface ITenantPool<TTenant, in TDataSource> : ITenantPool
{
    Task<TTenant> AddTenantAsync(string tenantName, TDataSource dataSource);

    Task RemoveTenantAsync(TTenant tenant);
}