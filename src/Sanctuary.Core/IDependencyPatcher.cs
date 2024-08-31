using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary;

/// <summary>
/// A patcher of a data access library. 
/// </summary>
/// <typeparam name="TDataAccessor">Type of service in MSDI, see <see cref="ServiceDescriptor.ServiceType"/>).</typeparam>
[PublicAPI]
public interface IDependencyPatcher<TDataAccessor>
{
    /// <summary>
    /// <para>
    /// Replace a <see cref="ServiceDescriptor.ServiceType">service</see> <typeparamref name="TDataAccessor"/>
    /// in a <paramref name="serviceCollection"/> with a patched <see cref="ServiceDescriptor.ImplementationFactory">
    /// implementation factory</see> that returns a version of service that uses tenants
    /// (<typeparamref name="TDataAccessor"/>) created for the test. Patcher will try its best to
    /// keep service as similar as possible, e.g. keep scope and service properties.
    /// </para>
    /// <para>
    /// Example:
    /// <example>
    /// <code>
    /// // Patcher will remove this original factory that resolved MyDbContext and replaces it with
    /// // a factory that resolves the service, but with a connection string to a tenant that was
    /// // created for the test (e.g. database <c>70dde648-c0ff-41d9-9ba39e8ab8a7</c>).
    /// var connectionString = "Server=.;Database=myDatabase;UserId=admin;Password=password;";
    /// services.AddDbContext&lt;MyDbContext&gt;(options => options.UseSqlServer(connectionString));
    /// </code>
    /// </example>
    /// </para>
    /// <para>
    /// Patcher is not magic and can fail in some cases, e.g.
    /// <example>
    /// <code>
    /// // EfCore is registered in a way that is unpatchable.
    /// services.AddDbContext&lt;MyDbContext&gt;(options => options.UseSqlServer(existingDbConnection));
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    /// <param name="serviceCollection">The service collection that will replace.</param>
    /// <exception cref="InvalidOperationException">When there is a service that can't be patched.
    /// <example>
    /// EfCore that uses UseSql(existingDbConnection)
    /// </example>
    /// </exception>
    void Register(IServiceCollection serviceCollection);
}