using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.SqlServer;

/// <summary>
/// A pool with a single an existing external ADO.NET SQL server.
/// </summary>
internal sealed class OneSqlServer : IComponentPool<SqlServerComponent>
{
    private readonly SqlServerComponent _component;

    internal OneSqlServer(string name, string connectionString)
    {
        _component = new SqlServerComponent(name, connectionString);
    }

    public IReadOnlyDictionary<string, SqlServerComponent> AcquireComponents(IReadOnlyDictionary<string, ComponentSpec> requestedComponents)
    {
        if (requestedComponents.Count != 1 || requestedComponents.Single().Key != _component.Name)
            throw new InvalidOperationException($"Pool contains only component '{_component.Name}'.");

        return new Dictionary<string, SqlServerComponent>
        {
            { requestedComponents.Single().Key, _component }
        };
    }
}