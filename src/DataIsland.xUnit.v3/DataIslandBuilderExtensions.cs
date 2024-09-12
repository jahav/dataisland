using System;
using DataIsland.xUnit.v3;
using JetBrains.Annotations;
using Xunit;

// ReSharper disable once CheckNamespace
// The same namespace as the DataIslandBuilder means IntelliSense will also offer extension methods
// immediately without need to import additional namespace. Because XunitTestContext is internal,
// it can only be created by extension methods. Offer them as nif they were instance methods of
// the DataIslandBuilder.
namespace DataIsland;

public static class DataIslandBuilderExtensions
{
    /// <inheritdoc cref="DataIslandBuilder.Build"/>
    [PublicAPI]
    public static IDataIsland BuildInProc(this DataIslandBuilder builder)
    {
        return Build(builder, new XUnitTestProvider());
    }

    /// <inheritdoc cref="DataIslandBuilder.Build"/>
    [PublicAPI]
    public static IDataIsland BuildAspNet(this DataIslandBuilder builder)
    {
        return Build(builder, new AspNetTestProvider());
    }

    private static IDataIsland Build(DataIslandBuilder builder, ICurrentTestProvider testProvider)
    {
        var testContext = TestContext.Current;
        if (testContext.PipelineStage == TestPipelineStage.Unknown)
        {
            throw new InvalidOperationException("""
                                                Method was called outside of xUnit test execution pipeline. A shared dictionary
                                                from xUnit framework that is used to glue various components together is not yet
                                                initialized.
                                                """);
        }

        return builder.Build(new XUnitTestContext(testContext.KeyValueStorage, testProvider));
    }
}
