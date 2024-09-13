using DataIsland;
using DataIsland.EfCore;
using DataIsland.SqlServer;
using WebApi.Tests;
using Xunit;

[assembly: AssemblyFixture(typeof(DataIslandFixture))]

namespace WebApi.Tests;

public class DataIslandFixture
{
    public DataIslandFixture()
    {
        var componentPool = SqlServerComponentFactory.ExistingSqlServer("Data Source=.;Integrated Security=True;TrustServerCertificate=True");
        var factory = new SqlDatabaseTenantFactory(@"c:\Temp\dataisland\files");

        Island = new DataIslandBuilder()
            .AddComponentPool("SQL Server", componentPool, factory)
            .AddTemplate("Template", opt =>
            {
                opt.AddComponent<SqlServerComponent, SqlServerSpec>("SQL Server");
                opt.AddTenant<SqlDatabaseTenant, SqlDatabaseSpec>("Database", "SQL Server");
                opt.AddDataAccess<AppDbContext>("Database");
            })
            .AddPatcher(new EfCorePatcher<AppDbContext>())
            .Build();
    }

    public IDataIsland Island { get; }
}

