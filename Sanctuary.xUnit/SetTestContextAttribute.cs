using System.Reflection;
using Xunit.v3;

namespace Sanctuary.xUnit;

public class ScopedTenantsAttribute : BeforeAfterTestAttribute
{
    public override async ValueTask Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var ctx = Xunit.TestContext.Current;

        var profileName = GetProfileName(test);
        var tenantsFactory = GetTenantsFactory(ctx, test);
        var tenants = await tenantsFactory.AddTenantsAsync(profileName);
        
        // Should be always null, so throw if there is anything.
        ctx.KeyValueStorage.Add(GetTenantsKey(ctx), tenants);
    }

    public override async ValueTask After(MethodInfo methodUnderTest, IXunitTest test)
    {
        var ctx = Xunit.TestContext.Current;

        var tenantsKey = GetTenantsKey(ctx);
        if (!ctx.KeyValueStorage.TryGetValue(tenantsKey, out var untypedTenants) || untypedTenants is null)
            throw new InvalidOperationException("Test didn't have tenants.");

        ctx.KeyValueStorage.Remove(tenantsKey);
        var tenants = (Dictionary<Type, Tenant>)untypedTenants;
        var tenantsFactory = GetTenantsFactory(ctx, test);
        await tenantsFactory.RemoveTenantsAsync(tenants);
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

    private static string GetProfileName(IXunitTest test)
    {
        if (!test.Traits.TryGetValue("Profile", out var profileTraits))
            return DataSetProfileAttribute.DefaultProfile;

        if (profileTraits.Count > 1)
            throw new InvalidOperationException("Not exactly one profile trait");

        var profileName = profileTraits.SingleOrDefault() ?? DataSetProfileAttribute.DefaultProfile;
        return profileName;
    }

    private static ITenantsFactory GetTenantsFactory(Xunit.TestContext context, IXunitTest test)
    {
        var fixtureType = GetFixtureType(test);
        var tenantsFactory = context.KeyValueStorage[$"{fixtureType.Name}-factory"] as ITenantsFactory;
        if (tenantsFactory is null)
            throw new InvalidOperationException("No factory");
        return tenantsFactory;
    }

    private static string GetTenantsKey(Xunit.TestContext context)
    {
        var testMethod = context.TestMethod;
        if (testMethod is null)
            throw new InvalidOperationException("Test method is null");

        var key = testMethod.UniqueID + "-tenants";
        return key;
    }
}