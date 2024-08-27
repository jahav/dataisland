using Sanctuary.SqlServer;
using Sanctuary.xUnit;

[assembly: AssemblyFixture(typeof(TenantFixture))]

namespace Sanctuary.xUnit;

/// <summary>
/// A fixture that is initialized before any test in teh assembly is run, and it is inserted into individual
/// class/collection fixtures. Its job is to initialize components before any tests are run and also clean up
/// components once all test did run.
/// </summary>
public class TenantFixture : IAsyncDisposable
{
    public TenantFixture()
    {
        var componentFactory = SqlServerComponentFactory.ExistingSqlServer("Data Source=.;Integrated Security=True;TrustServerCertificate=True");
        Pool = new SqlDatabaseTenantPool(componentFactory.GetComponent("DefaultComponent"), @"c:\Temp\sanctuary\files");
        var config = new SanctuaryConfig();
        config.RegisterComponentPool("DefaultComponent", Pool);
        config.AddProfile("DefaultProfile", opt =>
        {
            opt.AddDataAccess<TestDbContext>("DefaultTenant", "DefaultComponent")
                .WithDataSource(new SqlDatabaseDataSource().FromDisk(@"c:\Temp\sanctuary\test-001.bak"));
        });
        Config = config;

        TestContext = new TestContext();
    }

    public ITestContext TestContext { get; }

    public SqlDatabaseTenantPool Pool { get; set; }

    public SanctuaryConfig Config { get; }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
        //throw new NotImplementedException();
    }
}