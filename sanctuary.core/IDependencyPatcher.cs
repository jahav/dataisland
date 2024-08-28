using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary;

/// <summary>
/// A class that is used to access external component (e.g. EfCore accessing SqlServer or MassTransit accessing RabbitMQ).
/// </summary>
/// <typeparam name="TDataAccessor"></typeparam>
public interface IDependencyPatcher<TDataAccessor>
{
    void Register(IServiceCollection serviceCollection);
}