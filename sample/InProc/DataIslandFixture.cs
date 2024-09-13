using DataIsland.Demo;
using DataIsland.EfCore;
using DataIsland.SqlServer;
using Xunit;

// Register TenantLake as an assembly level fixture. It will therefore
// be initialized before tests (e.g. can spin up docker containers for
// components) and after (spin down the docker containers).
[assembly: AssemblyFixture(typeof(DataIslandFixture))]

namespace DataIsland.Demo;

/// <summary>
/// A fixture that is initialized before any test in the assembly is run, and it is inserted into individual
/// class/collection fixtures. Its job is to initialize components before any tests are run and also clean up
/// components once all test did run.
/// </summary>
public class DataIslandFixture : IAsyncLifetime
{
    public DataIslandFixture()
    {
        // An instance of SQL server to create/drop individual databases for tests.
        // This is a pool that might be able to create new components on demand
        // (or not).
        // It can be a single server and throw on any request of other component.
        // It can be adapter to an Azure with unlimited number of machines to spin
        // up many sql server to split the load.
        var componentPool = SqlServerComponentFactory.ExistingSqlServer("Data Source=.;Integrated Security=True;TrustServerCertificate=True");

        // Component pool is responsible for creating or dropping test databases
        // on one component (SQL Server). In essence, it is a factory for databases.
        var factory = new SqlDatabaseTenantFactory(
            // Directory on the SQL Server machine to store .mdf and .ldf files
            // for test databases.
            @"c:\Temp\dataisland\files");

        // Build a definition of possible configurations of external state.
        // Each view describes a state of external components (e.g.
        // * "nominal" view would only contain a database without data
        //   (=clean for any test).
        // * "performance" view could restore database full of data to
        //   check how it behaves under load.
        // * "blobs" would also create a tenant for blob storage that wouldn't
        //   be created in others, because it is only needed in some tests and
        //   is expansive to build (=not always necessary).
        // Each test can specify view using [ApplyTemplate("nominal")]
        // attribute.
        Island = new DataIslandBuilder()
            // Tenant lake will contain only one component - SQL Server defined
            // above.
            .AddComponentPool("DefaultComponent", componentPool, factory)

            // For default template, we request the following state of external components:
            .AddTemplate("DefaultTemplate", opt =>
            {
                opt.AddComponent<SqlServerComponent, SqlServerSpec>(
                    "DefaultComponent",
                    spec => spec
                        .WithCollation("SQL_Latin1_General_CP1_CI_AS")
                        .WithClrEnabled(true));

                // Create a single database on the default pool. It doesn't specify
                // special data source = use the default data source from the pool.
                opt.AddTenant<SqlDatabaseTenant, SqlDatabaseSpec>(
                    "DefaultTenant",
                    "DefaultComponent",
                    spec => spec
                        .WithDataSource(@"c:\Temp\dataisland\test-001.bak")
                        .WithMaxDop(1));

                // The patcher should patch QueryDbContext to hook into a database above.
                opt.AddDataAccess<QueryDbContext>("DefaultTenant");
            })

            // Patcher hooks into the MSDI and patches the resolving logic, so the
            // data access (QueryDbContext) uses tenant of the template. EfCorePatcher
            // replaces connection string with one that points to a test database
            // created for the test. It doesn't check that databases are compatible
            // to keep patcher agnostic to any relational database.
            .AddPatcher(new EfCorePatcher<QueryDbContext>())

            // Build checks that supplied configuration is valid and creates
            // immutable data island used by individual tests. Data island in
            // fixture is global because it is in assembly fixture.
            .Build();
    }

    public IDataIsland Island { get; }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
