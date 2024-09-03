using DataIsland.xUnit.v3;

// ReSharper disable once CheckNamespace
namespace DataIsland;

public static class DataIslandBuilderExtensions
{
    /// <inheritdoc cref="DataIslandBuilder.Build"/>
    public static IDataIsland Build(this DataIslandBuilder builder)
    {
        return builder.Build(new XUnitTestContext());
    }
}
