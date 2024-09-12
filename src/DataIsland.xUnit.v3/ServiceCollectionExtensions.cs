using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace DataIsland.xUnit.v3;

public static class ServiceCollectionExtensions
{
    public static void AddDataIsland<TFixture>(this IServiceCollection services, IDataIsland lake)
    {
        // Whether we are InProc or ASP.NET, just register everything, at worst it won't be used.
        // I want to keep API as similar as possible.
        RegisterAspNetMiddleware(services);

        // Register all data context factories.
        lake.PatchServices(services);

        // Add service to list of services, so data accessor registration factories
        // can resolve this context (that can resolve tenants for the test) and ask
        // to get a tenant required for the data access.
        services.AddSingleton<ITestContext>(lake.TestContext);

        // Add to values to ambient context, so we can initialize/release tenants even
        // in places where is no access to fixture (i.e. BeforeAfterTestAttribute).
        var fixtureName = typeof(TFixture).Name;

        var keyValueStorage = Xunit.TestContext.Current.KeyValueStorage;
        lock (keyValueStorage)
        {
            keyValueStorage[fixtureName] = lake;
            keyValueStorage[$"{fixtureName}-materializer"] = lake.Materializer;
        }
    }

    private static void RegisterAspNetMiddleware(IServiceCollection services)
    {
        // Register dependencies necessary to inject middle
        services.AddTransient<IStartupFilter, TestIdHeaderStartupFilter>();
        services.AddSingleton<TestIdMiddleware>();

        // TODO: This should be registered only for ASP.NET Core and will likely be changed when I deal with ITestContext
        services.AddSingleton<AspNetTestProvider>();
    }
}
