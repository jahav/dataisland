using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary;

internal class TenantLake(IMaterializer _materializer, ITestContext _testContext, IEnumerable<object> _patchers) : ITenantLake
{
    public IMaterializer Materializer => _materializer;

    public ITestContext TestContext => _testContext;

    public void PatchServices(IServiceCollection services)
    {
        foreach (var patcher in _patchers)
        {
            var interfaceType = patcher.GetType().GetInterface(typeof(IDependencyPatcher<>).Name);
            var registerMethod = interfaceType?.GetMethod(nameof(IDependencyPatcher<object>.Register));
            if (registerMethod is null)
                throw new UnreachableException($"Type '{patcher.GetType()}' is not a {typeof(IDependencyPatcher<>)}.");

            registerMethod.Invoke(patcher, [services]);
        }
    }
}