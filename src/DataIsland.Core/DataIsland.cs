using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using IDependencyPatcher = object;

namespace DataIsland;

internal class DataIsland(IMaterializer _materializer, ITestContext _testContext, IReadOnlyDictionary<Type, IDependencyPatcher> _patchers) : IDataIsland
{
    public IMaterializer Materializer => _materializer;

    public ITestContext TestContext => _testContext;

    public void PatchServices(IServiceCollection services)
    {
        foreach (var (dataAccessType, patcher) in _patchers)
        {
            // Single type can implement multiple patchers
            patcher.Register(dataAccessType, services);
        }
    }
}
