using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DataIsland.xUnit.v3;

/// <summary>
/// <para>
/// A handler that adds HTTP header with identifier of current test to the request sent to the test
/// server. The identifier is used by a middleware on the server to set the <see cref="ITestContext"/>
/// that is later used by patched service registrations to resolve data access classes that will
/// use tenants created for the test.
/// </para>
/// <para>
/// Generally, this handler should be added to HTTP client created by <c>WebApplicationFactory</c>,
/// through it can be used for any HTTP client created anywhere.
/// <code>
/// public SomeWebPageTests(CustomWebApplicationFactory&lt;Program&gt; factory)
/// {
///     _client = factory.CreateDefaultClient(new AddTestIdHandler());
/// }
/// </code>
/// </para>
/// </summary>
/// <remarks>
/// The header inserted by the handler is retrieved by <see cref="TestIdMiddleware"/>.
/// </remarks>
public class TestIdHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var currentTestContext = TestContext.Current;
        if (currentTestContext.PipelineStage != TestPipelineStage.TestExecution ||
            currentTestContext.TestMethod is not { } currentTestMethod)
        {
            throw new InvalidOperationException("""
                                                xUnit is currently not executing a test and handler thus can't determine a
                                                identifier of current test to send as a HTTP header to the test server. The
                                                identifier must be transmitted to the test server, so the patched MSDI service
                                                registrations know which tenants to use for data access libraries.
                                                """);
        }

        request.Headers.Add(SharedStorageConstants.TestIdHeaderName, currentTestMethod.UniqueID);
        return await base.SendAsync(request, cancellationToken);
    }
}
