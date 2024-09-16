using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DataIsland;

internal class LambdaPatcher<TDataAccess, TTenant>(Func<IServiceProvider, TTenant, TDataAccess> _factoryMethod)
    : IDependencyPatcher<TDataAccess>
    where TDataAccess : notnull
    where TTenant : class
{
    public void Register(IServiceCollection serviceCollection)
    {
        var originalRegistrations = serviceCollection.Where(x => x.ServiceType == typeof(TDataAccess)).ToList();
        if (originalRegistrations.Count == 0)
            throw new InvalidOperationException($"Service {typeof(TDataAccess)} isn't in the service collection - it can't be patched. Ensure the service is registered as a service before calling the patch method to patch the registration.");

        var replacements = new List<ServiceDescriptor>(originalRegistrations.Count);
        foreach (var registration in originalRegistrations)
        {
            replacements.Add(new ServiceDescriptor(typeof(TDataAccess), sp =>
            {
                var testContext = sp.GetRequiredService<ITestContext>();
                var tenant = testContext.GetTenant<TTenant>(typeof(TDataAccess));
                return _factoryMethod(sp, tenant);
            }, registration.Lifetime));
        }

        serviceCollection.RemoveAll(typeof(TDataAccess));
        serviceCollection.Add(replacements);
    }
}
