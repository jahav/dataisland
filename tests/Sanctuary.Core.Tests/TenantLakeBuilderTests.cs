using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Sanctuary.Core.Tests;

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
    // TODO:
    #endregion

    #region AddComponent
    // TODO: 
    #endregion

    #region Build
    // TODO: Verify configuration test
    #endregion

    [UsedImplicitly]
    public class TestDataAccess;
}