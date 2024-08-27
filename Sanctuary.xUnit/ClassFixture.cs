using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sanctuary.EfCore;

namespace Sanctuary.xUnit;

/// <summary>
/// A fixture that is created for each test class/collection.
/// </summary>
public class ClassFixture
{
    public ClassFixture(TenantFixture tenantFixture)
    {
        var services = new ServiceCollection();
        services.AddSingleton<SanctuaryConfig>(_ => tenantFixture.Config);

        services.AddDbContext<TestDbContext>(opt => opt.UseSqlServer("test"));

        services.AddSingleton<ITestContext>(tenantFixture.TestContext);

        new EfCoreAccessor<TestDbContext>().Register(services);
        ServiceProvider = services.BuildServiceProvider();

        var keyValueStorage = Xunit.TestContext.Current.KeyValueStorage;
        keyValueStorage[nameof(ClassFixture)] = tenantFixture.Config;
        keyValueStorage[$"{nameof(ClassFixture)}-factory"] = new TenantsFactory(tenantFixture.Config, new Dictionary<string, ITenantPool> { { "DefaultComponent", tenantFixture.Pool } });
    }

    public ServiceProvider ServiceProvider { get; }
}