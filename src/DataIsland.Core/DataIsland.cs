using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using IDependencyPatcher = object;

namespace DataIsland;

internal class DataIsland(
    IMaterializer _materializer,
    IReadOnlyDictionary<Type, IDependencyPatcher> _patchers,
    IReadOnlyCollection<IComponentPool> _pools) : IDataIsland
{
    public IMaterializer Materializer => _materializer;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var initTasks = _pools
            .Select(pool => pool.InitializeAsync(cancellationToken))
            .ToArray();

        await Task.WhenAll(initTasks);
    }

    public void PatchServices(IServiceCollection services)
    {
        foreach (var (dataAccessType, patcher) in _patchers)
        {
            // Single type can implement multiple patchers
            patcher.Register(dataAccessType, services);
        }
    }

    public async ValueTask DisposeAsync()
    {
        var disposeTasks = _pools
            .Select(pool => pool.DisposeAsync().AsTask())
            .ToArray();

        await Task.WhenAll(disposeTasks);
    }
}
