using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface ITenantFactory<TTenant, TComponent, TTenantSpec>
{
    Task<TTenant> AddTenantAsync(TComponent component, string tenantName, TTenantSpec spec);

    Task RemoveTenantAsync(TComponent component, TTenant tenant);
}