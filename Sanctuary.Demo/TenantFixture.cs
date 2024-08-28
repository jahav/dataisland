using Sanctuary.Demo;
using Sanctuary.EfCore;
using Sanctuary.SqlServer;
using Sanctuary.xUnit.v3;
using Xunit;

// Register TenantLake as an assembly level fixture. It will therefore
// be initialized before tests (e.g. can spin up docker containers for
// components) and after (spin down the docker containers).
[assembly: AssemblyFixture(typeof(TenantFixture))]

namespace Sanctuary.Demo;

/// <summary>
/// A fixture that is initialized before any test in the assembly is run, and it is inserted into individual
/// class/collection fixtures. Its job is to initialize components before any tests are run and also clean up
/// components once all test did run.
/// </summary>
public class TenantFixture
{
    public TenantFixture()
    {
        // An instance of SQL server to create/drop individual databases for tests.
        var componentPool = SqlServerComponentFactory.ExistingSqlServer("Data Source=.;Integrated Security=True;TrustServerCertificate=True");

        // Component pool is responsible for creating or dropping test databases
        // on one SQL Server.
        var pool = new SqlDatabaseTenantPool(
            // SQL Server of the tenant pool
            componentPool.GetComponent("DefaultComponent"),
            
            // Directory to store .mdf and .ldf files for test databases
            @"c:\Temp\sanctuary\files",
            
            // Default data source used to create database, unless tenant specifies otherwise.
            new SqlDatabaseDataSource().FromDisk(@"c:\Temp\sanctuary\test-001.bak"));

        // Build a definition of possible configurations of external state.
        // Each profile describes a state of external components (e.g.
        // * "nominal" profile would only contain a database without data
        //   (=clean for any test).
        // * "performance" profile could restore database full of data to
        //   check how it behaves under load.
        // * "blobs" would also create a tenant for blob storage that wouldn't
        //   be created in others, because it is only needed in some tests and
        //   is expansive to build (=not always necessary).
        // Each test can specify profile using [ScopedTenants("nominal")]
        // attribute.
        Lake = new TenantLakeBuilder()

            // Tenant lake will contain only one component - SQL Server defined
            // above.
            .AddComponentPool("DefaultComponent", pool)

            // For default profile, we request the following state of external components:
            .AddProfile("DefaultProfile", opt =>
            {
                // Create a single database on the default pool. It doesn't specify
                // special data source = use the default data source from the pool.
                opt.AddTenant<SqlDatabaseTenant>("DefaultTenant", "DefaultComponent");

                // The patcher should patch QueryDbContext to hook into a database above.
                opt.AddDataAccess<QueryDbContext>("DefaultTenant");
            })

            // Patcher hooks into the MSDI and patches the resolving logic, so the
            // data access (QueryDbContext) uses tenant of the profile. EfCorePatcher
            // replaces connection string with one that points to a test database
            // created for the test. It doesn't check that databases are compatible
            // to keep patcher agnostic to any relational database.
            .AddPatcher(new EfCorePatcher<QueryDbContext>())

            // Build checks that supplied configuration is valid and creates
            // immutable tenant lake used by individual tests. Tenant lake in
            // fixture is global because it is in assembly fixture.
            // The supplied context is a glue/ambient context that connects
            // the created tenant lake to rest of xUnit infrastructure.
            .Build(new XUnitTestContext());
    }

    public ITenantLake Lake { get; }
}
