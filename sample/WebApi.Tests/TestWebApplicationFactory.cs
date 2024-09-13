using DataIsland.xUnit.v3;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace WebApi.Tests;

public class TestWebApplicationFactory<TProgram>(DataIslandFixture _fixture) : WebApplicationFactory<TProgram>
    where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddDataIslandAspNet<TestWebApplicationFactory<TProgram>>(_fixture.Island);
        });
    }
}
