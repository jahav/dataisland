using System;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DataIsland;

internal static class DynamicCaller
{
    public static void Register(this object patcher, Type dataAccessType, IServiceCollection services)
    {
        var patcherInterface = typeof(IDependencyPatcher<>).MakeGenericType(dataAccessType);
        var registerMethod = patcherInterface.GetMethod("Register");
        Debug.Assert(registerMethod is not null);
        registerMethod.Invoke(patcher, [services]);
    }

    public static async Task<IDictionary> AcquireComponentsAsync(this object componentPool, object componentSpecs)
    {
        var componentPoolInterface = componentPool.GetType().GetInterface(typeof(IComponentPool<,>).Name);
        Debug.Assert(componentPoolInterface is not null);
        var getComponentMethod = componentPoolInterface.GetMethod("AcquireComponentsAsync");
        Debug.Assert(getComponentMethod is not null);
        var task = (Task?)getComponentMethod.Invoke(componentPool, [componentSpecs]);
        Debug.Assert(task is not null);
        await task;
        var resultProperty = task.GetType().GetProperty("Result");
        Debug.Assert(resultProperty is not null);

        // TODO: Shouldn't use IDictionary, but IReadOnlyDictionary<string, TComponent>
        var acquiredComponents = (IDictionary?)resultProperty.GetValue(task);
        Debug.Assert(acquiredComponents is not null);
        return acquiredComponents;
    }

    public static async Task<object> AddTenantAsync(this object tenantFactory, object component, object tenantSpec)
    {
        // Actually return Task<TTenant>
        var taskWithResult = await InvokeTenantFactoryMethod(tenantFactory, "AddTenantAsync", [component, tenantSpec]);
        var resultProperty = taskWithResult.GetType().GetProperty("Result");
        Debug.Assert(resultProperty is not null);
        var tenant = resultProperty.GetValue(taskWithResult);
        Debug.Assert(tenant is not null);
        return tenant;
    }

    public static async Task RemoveTenantAsync(this object tenantFactory, object component, object tenant)
    {
        await InvokeTenantFactoryMethod(tenantFactory, "RemoveTenantAsync", [component, tenant]);
    }

    private static async ValueTask<Task> InvokeTenantFactoryMethod(object tenantFactory, string methodName, object[] args)
    {
        var tenantFactoryInterface = tenantFactory.GetType().GetInterface(typeof(ITenantFactory<,,>).Name);
        Debug.Assert(tenantFactoryInterface is not null);
        var method = tenantFactoryInterface.GetMethod(methodName);
        Debug.Assert(method is not null);
        var task = (Task?)method.Invoke(tenantFactory, args);
        Debug.Assert(task is not null);
        await task;
        return task;
    }
}
