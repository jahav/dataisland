﻿using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sanctuary.xUnit.v3;

[PublicAPI]
public class XUnitTestContext : ITestContext
{
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

            if (untypedDataAccessMap is not IReadOnlyDictionary<Type, TenantInfo> dataAccessMap)
                throw new InvalidOperationException("Incorrect type for data access map");

            return (TTenant)dataAccessMap[dataAccessType].Instance;
        }
    }
}
