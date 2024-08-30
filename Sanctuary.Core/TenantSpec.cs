using System;

namespace Sanctuary;

internal readonly record struct TenantSpec(Type TenantType, string ComponentName, object? DataSource);