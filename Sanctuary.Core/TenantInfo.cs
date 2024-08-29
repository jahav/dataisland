using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class TenantInfo
{
    internal TenantInfo(object instance, string tenantName, string componentName, object component, HashSet<Type> dataAccess)
    {
        Instance = instance;
        TenantName = tenantName;
        ComponentName = componentName;
        Component = component;
        DataAccess = dataAccess;
    }

    /// <summary>
    /// Tenant instance.
    /// </summary>
    public object Instance { get; }

    /// <summary>
    /// Tenant name.
    /// </summary>
    public string TenantName { get; }

    /// <summary>
    /// Name of the component where tenant was created.
    /// </summary>
    public string ComponentName { get; }

    /// <summary>
    /// Component where tenant was created.
    /// </summary>
    public object Component { get; }

    /// <summary>
    /// Data access types that could potentially use tenant as data source.
    /// </summary>
    public IReadOnlyCollection<Type> DataAccess { get; }
}