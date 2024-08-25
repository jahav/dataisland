using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sanctuary;

public interface ITestContext
{

    public string TestId { get; }
    public string ProfileName { get; }
    bool TryGetTenant(string tenantName, [NotNullWhen(true)] out object? tenant);

    void AddTenant(string tenantName, object tenant);
}

public interface ITestTenantProvider
{
    TTenant GetOrAddTenant<TDataAccess, TTenant, TDataSource>(SanctuaryConfig config)
        where TDataAccess : class;
}

