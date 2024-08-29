using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface ITenantFactory<TTenant, in TDataSource> : ITenantFactory
{
    Task<TTenant> AddTenantAsync(string tenantName, TDataSource? dataSource);

    Task RemoveTenantAsync(TTenant tenant);
}