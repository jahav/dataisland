using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary;

public interface ITenantLake
{
    ITenantsFactory Factory { get; }

    ITestContext TestContext { get; }

    void PatchServices(IServiceCollection services);
}