using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary.xUnit
{
    public class TestTenantProvider : ITestTenantProvider
    {
        private readonly IServiceProvider _sp;
        private readonly ITestContext _testContext;

        public TestTenantProvider(IServiceProvider sp, ITestContext testContext)
        {
            _sp = sp;
            _testContext = testContext;
        }

        public TTenant GetOrAddTenant<TDataAccess, TTenant, TDataSource>(SanctuaryConfig config)
            where TDataAccess : class
        {
            // Get profile of currently running test.
            var profile = config.GetProfile(_testContext.ProfileName);

            DataAccessConfig<TDataAccess> dataAccess = profile.GetDataAccess<TDataAccess>();

            var tenantName = dataAccess.TenantName;
            var componentName = dataAccess.ComponentName;
            var untypedDataSource = dataAccess.DataSource;
            if (untypedDataSource is null)
                throw new InvalidOperationException();

            if (untypedDataSource is not TDataSource dataSource)
                throw new InvalidOperationException();

            if (_testContext.TryGetTenant(tenantName, out var tenant))
            {
                return (TTenant)tenant;
            }

            var tenantPool = _sp.GetRequiredService<ITenantPool<TTenant, TDataSource>>();
            var newTenant = tenantPool.AddTenant(tenantName, componentName, dataSource);
            _testContext.AddTenant(tenantName, newTenant);
            return newTenant;
        }

        public void CleanUpTenants()
        {

        }
    }
}
