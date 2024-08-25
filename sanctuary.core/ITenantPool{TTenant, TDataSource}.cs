namespace Sanctuary;

public interface ITenantPool<TTenant, in TDataSource> : ITenantPool
{
    TTenant AddTenant(string tenantName, string componentName, TDataSource dataSource);

    void RemoveTenant(TTenant tenant);
}