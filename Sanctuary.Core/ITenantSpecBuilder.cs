using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public interface ITenantSpecBuilder<TTenant>
{
    ITenantSpecBuilder<TTenant> WithDataSource<TDataSource>(TDataSource data);
}