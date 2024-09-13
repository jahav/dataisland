using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace DataIsland;

/// <summary>
/// An assembly fixture that set-up tenants before each test, clean them up afterward and
/// provides the tenants to data access libraries during the test run.
/// </summary>
[PublicAPI]
public interface IDataIsland : IAsyncDisposable
{
    /// <summary>
    /// A materializer that the testing framework uses to set up tenants before each test and clean
    /// them up afterward. It leverages the test's template attribute to determine which tenants to
    /// create.
    /// </summary>
    IMaterializer Materializer { get; }

    /// <summary>
    /// Initialize the data island, it means mostly pools.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch all data access services with patchers added through <see cref="DataIslandBuilder.AddPatcher{TDataAccess}"/>.
    /// The patched services will use tenants set up before each test instead of the originally
    /// specified tenants.
    /// </summary>
    /// <param name="services">Service collection whose services are replaced.</param>
    void PatchServices(IServiceCollection services);
}
