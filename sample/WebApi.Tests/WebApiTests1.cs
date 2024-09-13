using System.Net;
using DataIsland.xUnit.v3;
using Xunit;

namespace WebApi.Tests;

[ApplyTemplate("Template")]
public class WebApiTests1 : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public WebApiTests1(TestWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _output = output;
        _client = factory.CreateDefaultClient(new TestIdHandler());
    }

    [Fact]
    public async Task Database_is_created_for_each_test()
    {
        // Act - Get database name
        var defaultPage = await _client.GetAsync("/db-name");

        // Assert - we received a database name and instead of DemoDb (specified in appsettings.json)
        // it is a GUID. GUID is a clear sign that database was created for this test.
        var dbName= await defaultPage.Content.ReadAsStringAsync();
        _output.WriteLine($"Received text is '{dbName}'.");
        Assert.True(Guid.TryParse(dbName, out _), $"Received text is not GUID: '{dbName}'.");
        Assert.Equal(HttpStatusCode.OK, defaultPage.StatusCode);
    }
}
