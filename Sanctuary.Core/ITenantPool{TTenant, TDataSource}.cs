using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface ITenantPool<TTenant, in TDataSource> : ITenantPool
{
    Task<TTenant> AddTenantAsync(string tenantName, TDataSource? dataSource);

    Task RemoveTenantAsync(TTenant tenant);
}