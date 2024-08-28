namespace Sanctuary;

internal record TenantLake(ITenantsFactory Factory, ITestContext TestContext) : ITenantLake;