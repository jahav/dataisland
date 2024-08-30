using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary;

public interface ITenantLake
{
    IMaterializer Materializer { get; }

    ITestContext TestContext { get; }

    void PatchServices(IServiceCollection services);
}