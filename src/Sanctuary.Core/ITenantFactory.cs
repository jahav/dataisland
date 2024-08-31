using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Sanctuary;

/// <summary>
/// A factory that creates and removes tenants on a component.
/// </summary>
/// <typeparam name="TTenant">Type of tenant this factory is creating.</typeparam>
/// <typeparam name="TComponent">The component where this factory is creating <typeparamref name="TTenant"/>.</typeparam>
/// <typeparam name="TTenantSpec">The tenant specification, each tenant created by the factory will satisfy the specification.</typeparam>
[PublicAPI]
public interface ITenantFactory<TTenant, in TComponent, in TTenantSpec>
    where TTenantSpec : TenantSpec<TTenant>
{
    /// <summary>
    /// Create a new tenant that doesn't conflict with any other tenant on a <paramref name="component"/>.
    /// </summary>
    /// <param name="component">Component where tenant will be created.</param>
    /// <param name="spec">Tenant specification that describes how should returned tenant look like.</param>
    /// <returns>Created tenant.</returns>
    Task<TTenant> AddTenantAsync(TComponent component, TTenantSpec spec);

    /// <summary>
    /// Remove a tenant from component.
    /// </summary>
    /// <param name="component">Component from which should tenant be removed.</param>
    /// <param name="tenant">Tenant to remove.</param>
    /// <returns>
    /// <c>true</c> if tenant was found on component and removed, <c>false</c> when tenant wasn't
    /// found on the component.
    /// </returns>
    Task<bool> RemoveTenantAsync(TComponent component, TTenant tenant);
}