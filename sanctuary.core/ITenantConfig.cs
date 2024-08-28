using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface ITenantConfig<TTenant>
{
    ITenantConfig<TTenant> WithDataSource<TDataSource>(TDataSource data);
}