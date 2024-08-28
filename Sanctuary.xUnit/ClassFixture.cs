using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary.xUnit;

/// <summary>
/// A fixture that is created for each test class/collection.
/// </summary>
public class ClassFixture
{
    public ClassFixture(TenantFixture tenantFixture)
    {
        var services = new ServiceCollection();
        services.AddDbContext<QueryDbContext>(opt => opt.UseSqlServer("test"));

        // Needs to be last, because it overrides service registrations
        // of data access services.
        services.AddSanctuary<ClassFixture>(tenantFixture.Lake);

        ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; }
}