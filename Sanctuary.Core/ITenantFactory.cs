using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface ITenantFactory<TTenant, TComponent, in TDataSource>
{
    Task<TTenant> AddTenantAsync(TComponent component, string tenantName, TDataSource? dataSource);

    Task RemoveTenantAsync(TComponent component, TTenant tenant);
}