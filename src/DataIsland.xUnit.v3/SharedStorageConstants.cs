using Xunit;

namespace DataIsland.xUnit.v3;

internal static class SharedStorageConstants
{
    /// <summary>
    /// Name of a header that is sent from test case HTTP client to test server. It contains a
    /// unique identifier of currently running test. The name is used to create a key to
    /// <see cref="TestContext.KeyValueStorage"/> dictionary that stores tenant map created by
    /// <see cref="ApplyTemplateAttribute"/>.
    /// </summary>
    internal static readonly string TestIdHeaderName = "X-DataIsland-Unique-TestId";

    /// <summary>
    /// Get a key to shared storage that contains <c>IReadOnlyDictionary&lt;Type, Tenant&gt;</c>.
    /// The key is a type of data access, the value is a <see cref="Tenant"/> created for current
    /// test.
    /// </summary>
    /// <param name="testId">Identifier of current test.</param>
    /// <returns>Key to shared storage.</returns>
    internal static string GetDataAccessMapKey(string testId)
    {
        return testId + "-data-access-map";
    }

    /// <summary>
    /// Get key to shared storage that contains <see cref="IMaterializer"/>. Each class/collection
    /// has its own materializer, so multiple collections can materialize at the same time (tests
    /// within a collection never run in parallel).
    /// </summary>
    /// <param name="fixtureName">Name of a fixture, not DataIsland assembly fixture, but a collection/class fixture.</param>
    internal static string GetMaterializerKey(string fixtureName)
    {
        return fixtureName + "-materializer";
    }
}
