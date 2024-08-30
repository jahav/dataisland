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

    internal OneSqlServer(string connectionString)
    {
        _component = new SqlServerComponent(connectionString);
    }

    public IReadOnlyDictionary<string, SqlServerComponent> AcquireComponents(IReadOnlyDictionary<string, ComponentSpec> requestedComponents)
    {
        if (requestedComponents.Count != 1)
            throw new InvalidOperationException("Pool contains only one component. You must construct additional pylons.");

        var (componentName, componentSpec) = requestedComponents.Single();
        if (componentSpec.ComponentType != typeof(SqlServerComponent))
            throw new InvalidOperationException("Incorrect component type.");

        return new Dictionary<string, SqlServerComponent>
        {
            { componentName, _component }
        };
    }
}