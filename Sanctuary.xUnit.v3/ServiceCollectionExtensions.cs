using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary.xUnit.v3;

public static class ServiceCollectionExtensions
{
    public static void AddSanctuary<TFixture>(this IServiceCollection services, ITenantLake lake)
    {
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
        keyValueStorage[fixtureName] = lake;
        keyValueStorage[$"{fixtureName}-factory"] = lake.Factory;
    }
}

/*
TODO: Fix an exception
Xunit.Sdk.TestPipelineException
   Class fixture type 'Sanctuary.xUnit.ClassFixture' threw in its constructor
      at Xunit.v3.FixtureMappingManager.GetFixture(Type fixtureType) in D:\a\xunit\xunit\src\xunit.v3.core\Utility\FixtureMappingManager.cs:line 184
      at Xunit.v3.FixtureMappingManager.InitializeAsync(IReadOnlyCollection`1 fixtureTypes) in D:\a\xunit\xunit\src\xunit.v3.core\Utility\FixtureMappingManager.cs:line 226
      at Xunit.v3.ExceptionAggregator.RunAsync(Func`1 code) in D:\a\xunit\xunit\src\xunit.v3.core\Exceptions\ExceptionAggregator.cs:line 120
   
   System.InvalidOperationException
   Operations that change non-concurrent collections must have exclusive access. A concurrent update was performed on this collection and corrupted its state. The collection's state is no longer correct.
      at System.Collections.Generic.Dictionary`2.TryInsert(TKey key, TValue value, InsertionBehavior behavior)
      at System.Collections.Generic.Dictionary`2.set_Item(TKey key, TValue value)
      at Sanctuary.SanctuaryXunitExtensions.AddSanctuary[TFixture](ServiceCollection services, ITenantLake lake) in C:\Users\havli\source\repos\sancturary\Sanctuary.xUnit\SanctuaryXunitExtensions.cs:line 24
      at Sanctuary.xUnit.ClassFixture..ctor(TenantFixture tenantFixture) in C:\Users\havli\source\repos\sancturary\Sanctuary.xUnit\ClassFixture.cs:line 18
      at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
      at System.Reflection.MethodBaseInvoker.InvokeDirectByRefWithFewArgs(Object obj, Span`1 copyOfArgs, BindingFlags invokeAttr)
   

 */ 