using System;

namespace Sanctuary;

internal readonly record struct TenantConfig(Type TenantType, string ComponentName, object? DataSource);