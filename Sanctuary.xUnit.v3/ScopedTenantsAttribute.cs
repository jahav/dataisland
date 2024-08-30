using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.v3;

namespace Sanctuary.xUnit.v3;

public class ScopedTenantsAttribute : BeforeAfterTestAttribute
{
    private readonly string _templateName;
    
    public ScopedTenantsAttribute() : this("DefaultTemplate")
    {
    }

    public ScopedTenantsAttribute(string templateName)
    {
        _templateName = templateName;
    }

    public override async ValueTask Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var ctx = Xunit.TestContext.Current;

        var materializationFactory = GetMaterializer(ctx, test);
        var tenants = await materializationFactory.MaterializeTenantsAsync(_templateName);
        var dataAccessMap = tenants
            .SelectMany(tenantInfo => tenantInfo.DataAccess.Select(dataAccess => (dataAccess, tenantInfo)))
            .ToDictionary(x => x.dataAccess, x => x.tenantInfo);

        // The KeyValueStorage should never contain the test keys at this point,
        // so use Add in order to throw if there is anything. Also use lock,
        // because it is a normal collection, not concurrency safe collection.
        // The KeyValueStorage can therefore throw exception if multiple threads
        // try to modify it at the same time.
        lock (ctx.KeyValueStorage)
        {
            ctx.KeyValueStorage.Add(GetTenantsKey(ctx), tenants);
            ctx.KeyValueStorage.Add(GetDataAccessMapKey(ctx), dataAccessMap);
        }
    }

    public override async ValueTask After(MethodInfo methodUnderTest, IXunitTest test)
    {
        var ctx = Xunit.TestContext.Current;
        object? untypedTenants;
        lock (ctx.KeyValueStorage)
        {
            var tenantsKey = GetTenantsKey(ctx);
            if (!ctx.KeyValueStorage.TryGetValue(tenantsKey, out untypedTenants) || untypedTenants is null)
                throw new InvalidOperationException("Test didn't have tenants.");

            ctx.KeyValueStorage.Remove(tenantsKey);
            ctx.KeyValueStorage.Remove(GetDataAccessMapKey(ctx));
        }

        var tenants = (IEnumerable<TenantInfo>)untypedTenants;
        var materializer = GetMaterializer(ctx, test);
        await materializer.DematerializeTenantsAsync(tenants);
    }

    private static Type GetFixtureType(IXunitTest test)
    {
        // xUnit requires that all classes have only one ctor, otherwise it is an error.
        var ctor = test.TestMethod.TestClass.Class.GetConstructors().Single();
        var ctorParam = ctor.GetParameters().SingleOrDefault();
        if (ctorParam is null)
            throw new InvalidOperationException("No fixture");

        var fixtureType = ctorParam.ParameterType;
        return fixtureType;
    }

    private static IMaterializer GetMaterializer(Xunit.TestContext context, IXunitTest test)
    {
        var fixtureType = GetFixtureType(test);
        var materializer = context.KeyValueStorage[$"{fixtureType.Name}-materializer"] as IMaterializer;
        if (materializer is null)
            throw new InvalidOperationException("No materializer");
        return materializer;
    }

    private static string GetTenantsKey(Xunit.TestContext context)
    {
        return GetTestKey(context, "-tenants");
    }

    private static string GetDataAccessMapKey(Xunit.TestContext context)
    {
        return GetTestKey(context, "-data-access-map");
    }

    private static string GetTestKey(Xunit.TestContext context, string suffix)
    {
        var testMethod = context.TestMethod;
        if (testMethod is null)
            throw new InvalidOperationException("Test method is null");

        var key = testMethod.UniqueID + suffix;
        return key;
    }
}
