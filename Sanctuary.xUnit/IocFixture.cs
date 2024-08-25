using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sanctuary.EfCore;
using Sanctuary.SqlServer;

namespace Sanctuary.xUnit;

public class IocFixture
{
    public IocFixture()
    {
        var componentFactory = SqlServerComponentFactory.ExistingSqlServer("Data Source=.;Integrated Security=True;TrustServerCertificate=True");
        var pool = new SqlDatabaseTenantPool(componentFactory, @"c:\Temp\sanctuary\files");
        var config = new SanctuaryConfig();
        config.RegisterComponentPool("DefaultComponent", pool);
        config.AddProfile("DefaultProfile", opt =>
        {
            opt.AddDataAccess<TestDbContext>("DefaultTenant", "DefaultComponent")
                .WithDataSource(new SqlDatabaseDataSource().FromDisk(@"c:\Temp\sanctuary\test-001.bak"));
        });



        var services = new ServiceCollection();
        services.AddSingleton<SanctuaryConfig>(_ => config);

        services.AddDbContext<TestDbContext>(opt => opt.UseSqlServer("test"));

        services.AddSingleton<ITestTenantProvider, TestTenantProvider>();

        services.AddSingleton<ITestContext>(_ => TestContext.Instance);
        services.AddSingleton<ITenantPool<SqlDatabaseTenant, SqlDatabaseDataSource>>(pool);

        new EfCoreAccessor<TestDbContext>().Register(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; }
}