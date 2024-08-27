using System;
using System.Collections.Generic;

namespace Sanctuary;

/// <summary>
/// <para>
/// An interface that abstracts away a concrete testing framework context. DI
/// will use this class to get actual tenant for a data access. The test
/// context only holds tenants, it doesn't create them. The tenants are created
/// before the test is run (generally using testing framework infrastructure)
/// and released after test is run.
/// </para>
/// <para>
/// There is one instance per sanctuary config, and it must be registered to DI,
/// so it can fulfill its job of bridging concrete testing framework and generic
/// data accessor patcher. It can't implement <see cref="IDisposable"/> or
/// <see cref="IAsyncDisposable"/>, because it is registered to multiple DI
/// (e.g. multiple fixtures for xUnit) and no fixture can release it, because it
/// would be disposed for other fixtures.
/// </para>
/// </summary>
public interface ITestContext
{
    /// <summary>
    /// Get tenant to a data access. Before test is run, all tenants from
    /// configuration are created and stored in the test context.
    /// </summary>
    /// <remarks>The data access is used as a key to determine the correct tenant from <see cref="SanctuaryConfig"/>.</remarks>
    /// <typeparam name="TTenant">Type desired tenant.</typeparam>
    /// <param name="dataAccessType">Type of data access. Used to determine tenant from profile configuration.</param>
    /// <returns>Found tenant for the data access.</returns>
    /// <exception cref="KeyNotFoundException">If there is no tenant mapped for the <paramref name="dataAccessType"/>.</exception>
    TTenant GetTenant<TTenant>(Type dataAccessType)
        where TTenant : class;
}
