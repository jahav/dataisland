using Microsoft.Extensions.DependencyInjection;
using Sanctuary.EfCore;
using Sanctuary.xUnit;

namespace Sanctuary;

public static class SanctuaryXunitExtensions
{
    public static void AddSanctuary<TFixture>(this ServiceCollection services, ITenantLake lake)
    {
        // Register all data context factories.
        // TODO: Resolve from tenant lake registrations.
        new EfCoreAccessor<QueryDbContext>().Register(services);

        // Add service to list of services, so data accessor registration factories
        // can resolve this context (that can resolve tenants for the test) and ask
        // to get a tenant required for the data access.
        services.AddSingleton<ITestContext>(lake.TestContext);

        // Add to values to ambient context, so we can initialize/release tenants even
        // in places where is no access to fixture (i.e. BeforeAfterTestAttribute).
        var fixtureName = typeof(TFixture).Name;
        var keyValueStorage = Xunit.TestContext.Current.KeyValueStorage;
        keyValueStorage[fixtureName] = lake;
        keyValueStorage[$"{fixtureName}-factory"] = lake.Factory;
    }
}