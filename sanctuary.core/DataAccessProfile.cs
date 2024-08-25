using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public class DataAccessProfile
{
    private readonly Dictionary<Type, DataAccessConfig> _dataAccesses = new();

    public IDataAccessBuilder<TDataAccess> AddDataAccess<TDataAccess>(
        string tenantName = "Default",
        string componentName = "Default")
        where TDataAccess : class
    {
        var config = new DataAccessConfig<TDataAccess>
        {
            TenantName = tenantName,
            ComponentName = componentName,
        };
        _dataAccesses.Add(typeof(TDataAccess), config);
        return config;
    }

    internal DataAccessConfig<TDataAccess> GetDataAccess<TDataAccess>() where TDataAccess : class
    {
        return (DataAccessConfig<TDataAccess>)_dataAccesses[typeof(TDataAccess)];
    }
}