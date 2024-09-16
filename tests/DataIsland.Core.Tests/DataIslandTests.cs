using Microsoft.Extensions.DependencyInjection;
using Moq;
using static DataIsland.Core.Tests.DataIslandBuilderTests;

namespace DataIsland.Core.Tests;

public class DataIslandTests
{
    #region PatchServices

    [Fact]
    public void All_added_patches_are_applied_against_dependency_injection_container()
    {
        var patcher = new Mock<IDependencyPatcher<TestDataAccess>>();
        var sut = new DataIslandBuilder()
            .AddPatcher(patcher.Object)
            .Build();

        var services = new ServiceCollection();
        sut.PatchServices(services);

        patcher.Verify(x => x.Register(services), Times.Once);
    }

    [Fact]
    public void One_patcher_can_patch_different_data_access_types()
    {
        var patcherAsInterface1 = new Mock<IDependencyPatcher<TestDataAccess>>();
        var patcherAsInterface2 = patcherAsInterface1.As<IDependencyPatcher<TestDataAccess2>>();

        Assert.Same(patcherAsInterface1.Object, patcherAsInterface2.Object);
        var sut = new DataIslandBuilder()
            .AddPatcher(patcherAsInterface1.Object)
            .AddPatcher(patcherAsInterface2.Object)
            .Build();

        var services = new ServiceCollection();
        sut.PatchServices(services);

        patcherAsInterface1.Verify(x => x.Register(services), Times.Once);
        patcherAsInterface2.Verify(x => x.Register(services), Times.Once);
    }

    #endregion

    [Fact]
    public async Task DataIsland_initializes_and_disposes_of_pools()
    {
        var pool = new Mock<IComponentPool<DummyComponent, DummyComponentSpec>>();
        var factory = new Mock<ITenantFactory<DummyComponent, DummyTenant, DummyTenantSpec>>();
        var sut = new DataIslandBuilder()
            .AddComponentPool(pool.Object, factory.Object)
            .Build();

        pool.Verify(x => x.InitializeAsync(default), Times.Never);
        pool.Verify(x => x.DisposeAsync(), Times.Never);

        await sut.InitializeAsync();

        pool.Verify(x => x.InitializeAsync(default), Times.Once);
        pool.Verify(x => x.DisposeAsync(), Times.Never);

        await sut.DisposeAsync();

        pool.Verify(x => x.InitializeAsync(default), Times.Once);
        pool.Verify(x => x.DisposeAsync(), Times.Once);
    }
}
