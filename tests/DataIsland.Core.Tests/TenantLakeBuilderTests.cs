using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using DataIsland;

namespace DataIsland.Core.Tests;

public class TenantLakeBuilderTests
{
    #region PatchServices

    [Fact]
    public void All_added_patches_are_applied_against_dependency_injection_container()
    {
        var patcher = new Mock<IDependencyPatcher<TestDataAccess>>();
        var sut = new TenantLakeBuilder()
            .AddPatcher(patcher.Object)
            .Build(Mock.Of<ITestContext>());

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
        var sut = new TenantLakeBuilder()
            .AddPatcher(patcherAsInterface1.Object)
            .AddPatcher(patcherAsInterface2.Object)
            .Build(Mock.Of<ITestContext>());

        var services = new ServiceCollection();
        sut.PatchServices(services);

        patcherAsInterface1.Verify(x => x.Register(services), Times.Once);
        patcherAsInterface2.Verify(x => x.Register(services), Times.Once);
    }

    #endregion

    #region AddPatcher

    [Fact]
    public void Each_data_access_type_can_have_only_one_patcher()
    {
        var patcher1 = new Mock<IDependencyPatcher<TestDataAccess>>();
        var patcher2 = new Mock<IDependencyPatcher<TestDataAccess>>();
        var sut = new TenantLakeBuilder()
            .AddPatcher(patcher1.Object);

        var ex = Assert.Throws<ArgumentException>(() => sut.AddPatcher(patcher2.Object));
        Assert.StartsWith("An item with the same key has already been added.", ex.Message);
    }

    #endregion

    #region AddTenant

    [Fact]
    public void Each_component_type_has_exactly_one_pool()
    {
        var builder = new TenantLakeBuilder();

        builder.AddComponentPool("test",
            Mock.Of<IComponentPool<DummyComponent, ComponentSpec<DummyComponent>>>(),
            Mock.Of<ITenantFactory<DummyTenant, DummyComponent, TenantSpec<DummyTenant>>>());

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            // Try to register same component with a different name.
            builder.AddComponentPool("differentName",
                Mock.Of<IComponentPool<DummyComponent, ComponentSpec<DummyComponent>>>(),
                Mock.Of<ITenantFactory<DummyTenant, DummyComponent, TenantSpec<DummyTenant>>>());
        });
        Assert.Equal("Component pool for DataIsland.Core.Tests.TenantLakeBuilderTests+DummyComponent is already registered.", ex.Message);
    }

    [Fact]
    public void Each_component_pool_name_has_to_be_unique()
    {
        var builder = new TenantLakeBuilder();

        builder.AddComponentPool("tested name",
            Mock.Of<IComponentPool<DummyComponent, ComponentSpec<DummyComponent>>>(),
            Mock.Of<ITenantFactory<DummyTenant, DummyComponent, TenantSpec<DummyTenant>>>());

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            builder.AddComponentPool("tested name",
                Mock.Of<IComponentPool<DummyComponent2, ComponentSpec<DummyComponent2>>>(),
                Mock.Of<ITenantFactory<DummyTenant, DummyComponent2, TenantSpec<DummyTenant>>>());
        });
        Assert.Equal("Component name 'tested name' is already registered.", ex.Message);
    }

    #endregion

    #region AddComponent
    // TODO: 
    #endregion

    #region Build
    // TODO: Verify configuration test
    #endregion

    [UsedImplicitly]
    public class TestDataAccess;

    [UsedImplicitly]
    public class TestDataAccess2;

    [UsedImplicitly]
    public class DummyTenant;

    [UsedImplicitly]
    public class DummyComponent;

    [UsedImplicitly]
    public class DummyComponent2;
}
