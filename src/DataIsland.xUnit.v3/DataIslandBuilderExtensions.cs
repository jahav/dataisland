using DataIsland.xUnit.v3;

// ReSharper disable once CheckNamespace
namespace DataIsland;

public static class DataIslandBuilderExtensions
{
    /// <inheritdoc cref="TenantLakeBuilder.Build"/>
    public static ITenantLake Build(this TenantLakeBuilder builder)
    {
        return builder.Build(new XUnitTestContext());
    }
}
