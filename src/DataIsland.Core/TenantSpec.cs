using JetBrains.Annotations;

namespace Sanctuary;

[PublicAPI]
public abstract record TenantSpec<TTenant> : ITenantSpec
{
    // TODO: Required members  mean we can't use new constraint.
    public string ComponentName { get; internal init; } = "This value should never be used";
}

internal interface ITenantSpec
{
    string ComponentName { get; }
}