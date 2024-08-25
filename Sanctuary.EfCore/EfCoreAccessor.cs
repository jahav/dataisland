using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sanctuary.SqlServer;

namespace Sanctuary.EfCore;

[PublicAPI]
public class EfCoreAccessor<TDbContextImplementation> : IDataAccessorProvider<TDbContextImplementation>
    where TDbContextImplementation : DbContext
{
    public void Register(ServiceCollection serviceCollection)
    {
        // Standard methods that add DbContext implementations ultimately use
        // EntityFrameworkServiceCollectionExtensions.AddCoreServices to register
        // DbContextOptions<TContextImplementation>. The DbContext contains connection string

        // Must be scoped. DbContext is usually scoped. but in specific instances vis singleton.

        var serviceDescriptors = serviceCollection
            .Where(x => x.ServiceType == typeof(DbContextOptions<TDbContextImplementation>)).ToList();
        if (serviceDescriptors.Count == 0)
        {
            throw new InvalidOperationException(
                $"Unable to find DbContextOptions<{typeof(TDbContextImplementation).Name}>. Ensure the {typeof(TDbContextImplementation).Name} is registered before calling this method.");
        }

        if (serviceDescriptors.Count > 1)
        {
            throw new InvalidOperationException(
                $"Found {serviceDescriptors.Count} registrations of DbContextOptions<{typeof(TDbContextImplementation).Name}>. Ensure it is registered exactly once.");
        }

        var serviceDescriptor = serviceDescriptors.Single();
        if (serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
        {
            throw new InvalidOperationException(
                $"DbContextOptions<{typeof(TDbContextImplementation).Name}> has singleton lifetime. It must have either scoped or transient lifetime.");
        }

        //        DbContextOptions<TDbContextImplementation> a;
        //        var relationalOptions = a.GetExtension<RelationalOptionsExtension>();
        //        relationalOptions.WithConnectionString(connectionString);
        //        var extension = GetOrCreateExtension<SqlServerOptionsExtension>(optionsBuilder);
        //
        if (serviceDescriptor.ImplementationFactory is null)
        {
            throw new InvalidOperationException(
                $"DbContextOptions<{typeof(TDbContextImplementation).Name}> is not resolved through factory.");
        }

        Func<IServiceProvider, object> originalFactory = serviceDescriptor.ImplementationFactory;

        Func<IServiceProvider, object> newFactory = sp =>
        {
            var value = originalFactory(sp);
            DbContextOptions<TDbContextImplementation> typedValue =
                (DbContextOptions<TDbContextImplementation>)value;
            var relationalOptions = typedValue.Extensions.OfType<RelationalOptionsExtension>().Single();

            // get tenant

            // Get config
            var config = sp.GetRequiredService<SanctuaryConfig>();
            // Get test id
            var testContext = sp.GetRequiredService<ITestTenantProvider>();
            var tenant = testContext.GetOrAddTenant<TDbContextImplementation, SqlDatabaseTenant, SqlDatabaseDataSource>(config);


            var modified= relationalOptions.WithConnectionString(tenant.ConnectionString);
            return typedValue.WithExtension(modified);
        };

        // Remove old
        // Add new

        //serviceCollection.TryAdd(
        //    new ServiceDescriptor(
        //        typeof(DbContextOptions<TContextImplementation>),
        //        p => CreateDbContextOptions<TContextImplementation>(p, optionsAction),
        //        optionsLifetime));


        //serviceCollection.AddDbContext<>()
        var optionsLifetime = serviceDescriptor.Lifetime; // Keep the original lifetime.
        var newServiceDescriptor =
            new ServiceDescriptor(
                typeof(DbContextOptions<TDbContextImplementation>),
                p => newFactory(p),
                optionsLifetime);
        serviceCollection.Replace(newServiceDescriptor);
    }
}