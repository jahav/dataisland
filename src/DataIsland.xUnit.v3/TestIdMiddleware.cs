using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using static DataIsland.xUnit.v3.SharedStorageConstants;
namespace DataIsland.xUnit.v3;

/// <summary>
/// Middleware that takes <c>testId</c> sent by <see cref="TestIdHandler"/> from a xUnit test and
/// sets it to the <see cref="ICurrentTestProvider"/> for this request. The <see cref="ICurrentTestProvider"/>
/// is used by the <see cref="XUnitTestContext"/> to determine where in the shared storage are the tenants for
/// the current test.
/// </summary>
internal class TestIdMiddleware : IMiddleware
{
    [UsedImplicitly]

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Get testId sent by HTTP client from a test.
        if (!TryGetCurrentTestId(context, out var currentTestId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync($"""
                                               Header '{TestIdHeaderName}' must contain exactly one non-empty value.
                                               The header contains identifier of currently executed test, so MSDI on
                                               test server knows which tenants to use when resolving data access
                                               libraries.
                                               
                                               Make sure the HttpClient uses {nameof(TestIdHandler)}.
                                               Example: _client = factory.CreateDefaultClient(new AddTestIdHandler());
                                               """);
            return;
        }

        var untypedTestNameProvider = context.RequestServices.GetService(typeof(AspNetTestProvider));
        if (untypedTestNameProvider is not AspNetTestProvider testNameProvider)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync($$"""
                                                The MSDI doesn't contain service {{nameof(AspNetTestProvider)}}.

                                                Make sure you registered it to the WebApplicationFactory.
                                                Example:
                                                
                                                public class CustomWebApplicationFactory<TProgram>(DataIslandFixture _fixture)
                                                    : WebApplicationFactory<TProgram> where TProgram : class
                                                {
                                                  protected override void ConfigureWebHost(IWebHostBuilder builder)
                                                  {
                                                    builder.ConfigureTestServices(services =>
                                                    {
                                                      services.AddDataIsland<CustomWebApplicationFactory<TProgram>>(_fixture.Island);
                                                    });
                                                  }
                                                }
                                                """);
            return;
        }

        testNameProvider.SetTestId(currentTestId);

        try
        {
            await next(context);
        }
        finally
        {
            testNameProvider.SetTestId(null);
        }
    }

    private static bool TryGetCurrentTestId(HttpContext context, out string currentTestId)
    {
        currentTestId = string.Empty;
        if (!context.Request.Headers.TryGetValue(TestIdHeaderName, out var headerValues))
            return false;

        if (headerValues.Count != 1)
            return false;

        var currentTestKey = headerValues.Single();
        if (string.IsNullOrWhiteSpace(currentTestKey))
            return false;

        currentTestId = currentTestKey;
        return true;
    }
}
