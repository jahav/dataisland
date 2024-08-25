using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanctuary.xUnit
{
    internal class TestContext : ITestContext
    {
        internal static readonly TestContext Instance = new();

        //private readonly AsyncLocal<string?> _testId = new();
        //private readonly AsyncLocal<string?> _profileName = new();

        private readonly AsyncLocal<Dictionary<string, object>> _tenants = new();

        public string TestId
        {
            get => Xunit.TestContext.Current.TestMethod.UniqueID;
            //set => _testId.Value = value;
        }

        public string ProfileName
        {
            get
            {
                var c = Xunit.TestContext.Current.KeyValueStorage["ProfileName"];
                return c.ToString();
            }
            //set => _profileName.Value = value;
        }

        public bool TryGetTenant(string tenantName, [NotNullWhen(true)] out object? tenant)
        {
            var currentTenant = _tenants.Value;
            if (currentTenant is null)
                _tenants.Value = currentTenant = new();

            return currentTenant.TryGetValue(tenantName, out tenant);
        }

        public void AddTenant(string tenantName, object tenant)
        {
            var currentTenant = _tenants.Value;
            if (currentTenant is null)
                _tenants.Value = currentTenant = new();

            currentTenant.Add(tenantName, tenant);
        }
    }
}
