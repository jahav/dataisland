using DataIsland.xUnit.v3;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedServer;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);

public class ClassFixture
{
    public ClassFixture(DataIslandFixture dataIslandFixture)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer());

        services.AddDataIslandInProc<ClassFixture>(dataIslandFixture.Island);
        ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; }
}

[ApplyTemplate("default")]
public class DummyTest(ClassFixture _fixture) : IClassFixture<ClassFixture>
{
    [Fact]
    public void Test_so_assembly_fixture_is_initialized()
    {
        Assert.True(true);
    }
}
