using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNet.Testcontainers.Containers;
using Testcontainers.MsSql;

namespace DataIsland.SqlServer;

internal class DockerSqlServerPool(MsSqlContainer[] _msSqlContainers)
    : IComponentPool<SqlServerComponent, SqlServerSpec>
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var startTasks = _msSqlContainers
            .Select(c => c.StartAsync(cancellationToken))
            .ToArray();
        await Task.WhenAll(startTasks);
    }

    public async Task<IReadOnlyDictionary<string, SqlServerComponent>> AcquireComponentsAsync(IReadOnlyDictionary<string, SqlServerSpec> requestedComponents)
    {
        foreach (var container in _msSqlContainers)
        {
            var containerState = container.State;
            if (containerState != TestcontainersStates.Running)
            {
                throw new InvalidOperationException($$"""
                                                      Container {{container.Image.FullName}} is not in state {{nameof(TestcontainersStates.Running)}},
                                                      but in state {{containerState}}.

                                                      Make sure the data island was initialized. For xUnit, the pools are initialized
                                                      when data island is initialized. Mark the assembly fixture with IAsyncLifetime
                                                      and calls methods:
                                                      
                                                      public async ValueTask InitializeAsync()
                                                      {
                                                          await Island.InitializeAsync();
                                                      }
                                                      
                                                      public async ValueTask DisposeAsync()
                                                      {
                                                          await Island.DisposeAsync();
                                                      }
                                                      """);
            }
        }

        var result = new Dictionary<string, SqlServerComponent>();
        var candidates = _msSqlContainers.ToList();
        foreach (var (componentName, spec) in requestedComponents)
        {
            var found = false;
            foreach (var candidate in candidates)
            {
                var connectionString = candidate.GetConnectionString();
                var error = await SqlServerSpecChecker.CheckAsync(spec, connectionString);
                if (error is null)
                {
                    found = true;
                    result.Add(componentName, new SqlServerComponent(connectionString));
                    candidates.Remove(candidate);
                    break;
                }
            }

            if (!found)
                throw new InvalidOperationException($"Unable to find component for specs of '{componentName}'.");
        }

        return result;
    }

    public async ValueTask DisposeAsync()
    {
        // Parallelize dispose
        var disposeTasks = _msSqlContainers
            .Select(x => x.DisposeAsync().AsTask())
            .ToArray();
        await Task.WhenAll(disposeTasks);
    }
}
