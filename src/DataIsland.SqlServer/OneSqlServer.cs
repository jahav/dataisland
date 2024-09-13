using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataIsland.SqlServer;

/// <summary>
/// A pool with a single an existing external ADO.NET SQL server.
/// </summary>
internal sealed class OneSqlServer : IComponentPool<SqlServerComponent, SqlServerSpec>
{
    private readonly SqlServerComponent _component;

    internal OneSqlServer(string connectionString)
    {
        _component = new SqlServerComponent(connectionString);
    }

    public async Task<IReadOnlyDictionary<string, SqlServerComponent>> AcquireComponentsAsync(IReadOnlyDictionary<string, SqlServerSpec> requestedComponents)
    {
        if (requestedComponents.Count != 1)
            throw new InvalidOperationException("Pool contains only one component. You must construct additional pylons.");

        var (componentName, componentSpec) = requestedComponents.Single();
        if (componentSpec.ComponentType != typeof(SqlServerComponent))
            throw new InvalidOperationException("Incorrect component type.");

        var error = await SqlServerSpecChecker.CheckAsync(componentSpec, _component.ConnectionString);
        if (error is not null)
            throw new InvalidOperationException(error);

        return new Dictionary<string, SqlServerComponent>
        {
            { componentName, _component }
        };
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }
}
