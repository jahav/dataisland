using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace DataIsland;

internal class DataIsland(IMaterializer _materializer, ITestContext _testContext, IReadOnlyDictionary<Type, object> _patchers) : IDataIsland
{
    public IMaterializer Materializer => _materializer;

    public ITestContext TestContext => _testContext;

    public void PatchServices(IServiceCollection services)
    {
        foreach (var (dataAccessType, patcher) in _patchers)
        {
            // Single type can implement multiple patchers
            var patcherInterface = typeof(IDependencyPatcher<>).MakeGenericType(dataAccessType);
            var registerMethod = patcherInterface.GetMethod(nameof(IDependencyPatcher<object>.Register));
            Debug.Assert(registerMethod is not null);
            registerMethod.Invoke(patcher, [services]);
        }
    }
}
