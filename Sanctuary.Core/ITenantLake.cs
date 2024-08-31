using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary;

/// <summary>
/// An assembly fixture that set-up tenants before each test, clean them up afterward and
/// provides the tenants to data access libraries during the test run.
/// </summary>
[PublicAPI]
public interface ITenantLake
{
    /// <summary>
    /// A materializer that the testing framework uses to set up tenants before each test and clean
    /// them up afterward. It leverages the test's template attribute to determine which tenants to
    /// create.
    /// </summary>
    IMaterializer Materializer { get; }

    /// <summary>
    /// A glue object that connects the test framework with the MSDI service. It manages data for
    /// all running tests and provides values based on the specific test requesting them.
    /// 
    /// The testing framework stores the tenants (set up before each test) in the context.
    /// Dependency injection then uses the implementation factory (from <see cref="IDependencyPatcher{TDataAccessor}"/>)
    /// to return a data component that works with the tenants created for the test.
    ///
    /// Values are stored separately for each test, ensuring that data remains isolated and
    /// concurrent.
    /// </summary>
    ITestContext TestContext { get; }

    /// <summary>
    /// Patch all data access services with patchers added through <see cref="TenantLakeBuilder.AddPatcher{TDataAccess}"/>.
    /// The patched services will use tenants set up before each test from <see cref="TestContext"/>
    /// instead of the originally specified tenants.
    /// </summary>
    /// <param name="services">Service collection whose services are replaced.</param>
    void PatchServices(IServiceCollection services);
}