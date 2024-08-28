namespace Sanctuary;

public interface ITenantLake
{
    ITenantsFactory Factory { get; }
    ITestContext TestContext { get; }
}