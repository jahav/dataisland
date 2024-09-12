using System;
using System.Collections.Generic;

namespace DataIsland.xUnit.v3;

/// <summary>
/// A context that provides access to tenants created for the test. It is stored in DI and patched
/// service registrations use it to get tenant.
/// </summary>
/// <param name="sharedStorage">
/// A shared storage that contains all data access maps for all running tests. The
/// <see cref="ApplyTemplateAttribute"/> creates and stores data access map to the shared storage
/// and once test is done it removes them from the storage. This is single instance shared across
/// whole assembly and is not thread safe. Make sure to use
/// <code>lock (_sharedStorage) { _sharedStorage.Something(); } </code>.
/// </param>
/// <param name="testProvider">
/// A provider of current test id. It is used to determine which tenant map from
/// <paramref name="sharedStorage"/> should be used.
/// </param>
internal class XUnitTestContext(Dictionary<string, object?> sharedStorage, ICurrentTestProvider testProvider) : ITestContext
{
    private readonly Dictionary<string, object?> _sharedStorage = sharedStorage ?? throw new ArgumentNullException(nameof(sharedStorage));
    private readonly ICurrentTestProvider _testProvider = testProvider ?? throw new ArgumentNullException(nameof(testProvider));

    public bool HasMaterializedTemplate
    {
        get
        {
            // Are we even within a test?
            var testId = _testProvider.CurrentTestId;
            if (testId is null)
                return false;

            // Are we in a test that has [ApplyTemplate] attribute?
            lock (_sharedStorage)
            {
                var dataAccessMapKey = SharedStorageConstants.GetDataAccessMapKey(testId);
                return _sharedStorage.ContainsKey(dataAccessMapKey);
            }
        }
    }

    /// <inheritdoc />
    public TTenant GetTenant<TTenant>(Type dataAccessType)
        where TTenant : class
    {
        var currentTestId = _testProvider.CurrentTestId;
        if (currentTestId is null)
        {
            throw new InvalidOperationException("""
                                                Unable to determine currently running test.

                                                Make sure that: 
                                                * This `GetTenant` is called from a test method.
                                                * DataIsland TestContext was built for correct mode (`.BuildInProc()` for
                                                  tests running directly in the test method, `.BuildAspNet()` for ASP.NET
                                                  Core integration tests that run on test server.
                                                """);
        }

        var dataAccessMapKey = SharedStorageConstants.GetDataAccessMapKey(currentTestId);
        lock (_sharedStorage)
        {
            var dataAccessMap = (IReadOnlyDictionary<Type, Tenant>?)_sharedStorage[dataAccessMapKey];
            if (dataAccessMap is null)
            {
                throw new InvalidOperationException("""
                                                    Shared storage doesn't contain data access map.

                                                    Make sure that the test (either method or class) has [ApplyTemplate]
                                                    attribute.
                                                    """);
            }

            return (TTenant)dataAccessMap[dataAccessType].Instance;
        }
    }
}
