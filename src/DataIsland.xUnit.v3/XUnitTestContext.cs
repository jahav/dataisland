using System;
using System.Collections.Generic;

namespace DataIsland.xUnit.v3;

internal class XUnitTestContext : ITestContext
{
    public bool HasMaterializedTemplate
    {
        get
        {
            var ctx = Xunit.TestContext.Current;
            lock (ctx.KeyValueStorage)
            {
                return ctx.KeyValueStorage.ContainsKey(ctx.TestMethod!.UniqueID + "-data-access-map");
            }
        }
    }

    /// <inheritdoc />
    public TTenant GetTenant<TTenant>(Type dataAccessType)
        where TTenant : class
    {
        var ctx = Xunit.TestContext.Current;
        lock (ctx.KeyValueStorage)
        {
            var untypedDataAccessMap = ctx.KeyValueStorage[ctx.TestMethod!.UniqueID + "-data-access-map"];
            if (untypedDataAccessMap is null)
                throw new InvalidOperationException("No data access map.");

            if (untypedDataAccessMap is not IReadOnlyDictionary<Type, Tenant> dataAccessMap)
                throw new InvalidOperationException("Incorrect type for data access map");

            return (TTenant)dataAccessMap[dataAccessType].Instance;
        }
    }
}
