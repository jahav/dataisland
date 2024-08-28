using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sanctuary.SqlServer;

namespace Sanctuary.EfCore;

/// <typeparam name="TDbContext">The concrete type of DbContext that is going to be instantiated.</typeparam>
[PublicAPI]
public class EfCoreAccessor<TDbContext> : IDataAccessorProvider<TDbContext>
    where TDbContext : DbContext
{
    public void Register(IServiceCollection serviceCollection)
    {
        // Standard methods that add DbContext implementations ultimately use
        // EntityFrameworkServiceCollectionExtensions.AddCoreServices to register
        // DbContextOptions<TContextImplementation>. The DbContext contains connection string

        // Service must be scoped. DbContext is usually scoped, but in specific instances is singleton.
        var serviceDescriptors = serviceCollection
            .Where(x => x.ServiceType == typeof(DbContextOptions<TDbContext>)).ToList();
        if (serviceDescriptors.Count == 0)
        {
            throw new InvalidOperationException($"Unable to find DbContextOptions<{typeof(TDbContext).Name}>. Ensure the {typeof(TDbContext).Name} is registered before calling this method.");
        }

        if (serviceDescriptors.Count > 1)
        {
            throw new InvalidOperationException($"Found {serviceDescriptors.Count} registrations of DbContextOptions<{typeof(TDbContext).Name}>. Ensure it is registered exactly once.");
        }

        var serviceDescriptor = serviceDescriptors.Single();
        if (serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
        {
            throw new InvalidOperationException($"DbContextOptions<{typeof(TDbContext).Name}> has singleton lifetime. It must have either scoped or transient lifetime.");
        }

        if (serviceDescriptor.ImplementationFactory is null)
        {
            throw new InvalidOperationException($"DbContextOptions<{typeof(TDbContext).Name}> is not resolved through factory.");
        }

        Func<IServiceProvider, object> originalFactory = serviceDescriptor.ImplementationFactory;

        Func<IServiceProvider, object> newFactory = sp =>
        {
            var testContext = sp.GetRequiredService<ITestContext>();
            var tenant = testContext.GetTenant<SqlDatabaseTenant>(typeof(TDbContext));

            var originalOptions = (DbContextOptions<TDbContext>)originalFactory(sp);
            var relationalExtension = originalOptions.Extensions.OfType<RelationalOptionsExtension>().Single();
            var modifiedExtension = relationalExtension.WithConnectionString(tenant.ConnectionString);
            return originalOptions.WithExtension(modifiedExtension);
        };

        var newServiceDescriptor = new ServiceDescriptor(
            typeof(DbContextOptions<TDbContext>),
            p => newFactory(p),
            serviceDescriptor.Lifetime); // Keep the original lifetime.

        serviceCollection.Replace(newServiceDescriptor);
    }
}