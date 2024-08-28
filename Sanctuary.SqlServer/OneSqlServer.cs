using System;

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

    public SqlServerComponent GetComponent(string componentName)
    {
        if (componentName != _component.Name)
            throw new InvalidOperationException($"Pool contains only component '{_component.Name}'.");

        return _component;
    }
}