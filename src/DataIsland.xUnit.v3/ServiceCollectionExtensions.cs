using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using static DataIsland.xUnit.v3.SharedStorageConstants;

namespace DataIsland.xUnit.v3;

[SuppressMessage("ReSharper", "RedundantTypeArgumentsOfMethod", Justification = "Explicit type makes type fo registered service explicit.")]
public static class ServiceCollectionExtensions
{
    public static void AddDataIslandInProc<TFixture>(this IServiceCollection services, IDataIsland lake)
    {
        services.AddData<TFixture>(lake, new XUnitTestProvider());
    }

    public static void AddDataIslandAspNet<TFixture>(this IServiceCollection services, IDataIsland lake)
    {
        // Register dependencies necessary to inject ASP.NET middleware.
        services.AddTransient<IStartupFilter, TestIdHeaderStartupFilter>();
        services.AddSingleton<TestIdMiddleware>();

        // Ensure the provider is registered both and concrete type (used in TypeIdMiddleware) and as interface (used in patchers).
        var provider = new AspNetTestProvider();
        services.AddSingleton<AspNetTestProvider>(provider);
        services.AddData<TFixture>(lake, provider);
    }

    private static void AddData<TFixture>(this IServiceCollection services, IDataIsland lake, ICurrentTestProvider testProvider)
    {
        var testContext = TestContext.Current;
        if (testContext.PipelineStage == TestPipelineStage.Unknown)
        {
            throw new InvalidOperationException("""
                                                Method was called outside of xUnit test execution pipeline. A shared dictionary
                                                from xUnit framework that is used to glue various components together is not yet
                                                initialized.
                                                """);
        }

        var sharedStorage = testContext.KeyValueStorage;

        // Add service to list of services, so data accessor registration factories
        // can resolve this context (that can resolve tenants for the test) and ask
        // to get a tenant required for the data access.
        services.AddSingleton<ITestContext>(new XUnitTestContext(sharedStorage, testProvider));
        services.AddSingleton<ICurrentTestProvider>(testProvider);

        // Register all data context factories.
        lake.PatchServices(services);

        // Add to values to ambient context, so we can initialize/release tenants even
        // in places where is no access to fixture (i.e. BeforeAfterTestAttribute).
        var fixtureName = typeof(TFixture).Name;

        lock (sharedStorage)
        {
            sharedStorage[fixtureName] = lake;
            sharedStorage[GetMaterializerKey(fixtureName)] = lake.Materializer;
        }
    }
}
