using System;

namespace Sanctuary;

public abstract record TenantSpec<T>() : TenantSpec(typeof(T));

public abstract record TenantSpec(Type TenantType)
{
    // TODO: Required members  mean we can't use new constraint.
    public string ComponentName { get; init; } = "This value should never be used";
};