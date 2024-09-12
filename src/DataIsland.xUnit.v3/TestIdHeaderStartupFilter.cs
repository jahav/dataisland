using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace DataIsland.xUnit.v3;

/// <summary>
/// <para>
/// A startup filter that adds <see cref="TestIdMiddleware"/> to the beginning of the ASP.NET Core
/// pipeline. Startup filters are basically a requirement when test want to add a middleware to an
/// existing pipeline. The <c>.Configure</c> in a <c>WebApplicationFactory{TEntryPoint}</c>
/// overrides completely existing the middleware pipeline and is thus unsuitable for adding a
/// middleware.
/// </para>
/// <para>
/// <code>
///     protected override void ConfigureWebHost(IWebHostBuilder builder)
///     {
///       // DON'T DO THIS: Calling .Configure here replaces whatever middleware was existing in
///       // the original program. That means no routing and other things that were part of the
///       // original pipeline.
///       builder.Configure(app =>
///       {
///          method builder.Configure(app => app.UseMiddleware&lt;SomeMiddleware&gt; ());
///       });
///     }
/// </code>
/// </para>
/// </summary>
internal class TestIdHeaderStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.UseMiddleware<TestIdMiddleware>();
            next(builder);
        };
    }
}
