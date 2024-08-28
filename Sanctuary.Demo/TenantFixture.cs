using Sanctuary.Demo;
using Sanctuary.EfCore;
using Sanctuary.SqlServer;

[assembly: AssemblyFixture(typeof(TenantFixture))]

namespace Sanctuary.Demo;

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
        var pool = new SqlDatabaseTenantPool(
            componentFactory.GetComponent("DefaultComponent"),
            @"c:\Temp\sanctuary\files",
            new SqlDatabaseDataSource().FromDisk(@"c:\Temp\sanctuary\test-001.bak"));
        
        Lake = new TenantLakeBuilder()
            .AddComponentPool("DefaultComponent", pool)
            .AddProfile("DefaultProfile", opt =>
            {
                opt.AddDataAccess<QueryDbContext>("DefaultTenant");
                opt.AddTenant<SqlDatabaseTenant>("DefaultTenant", "DefaultComponent");
            })
            .AddPatcher(new EfCorePatcher<QueryDbContext>())
            .Build(new Sanctuary.xUnit.v3.TestContext());
    }

    public ITenantLake Lake { get; }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
