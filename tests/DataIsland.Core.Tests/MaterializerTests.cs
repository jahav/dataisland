using Moq;

namespace DataIsland.Core.Tests;

public class MaterializerTests
{
    [Fact]
    public async Task MaterializeTenantsAsync_asks_for_component_and_then_creates_tenant_from_factory()
    {
        var component = new DummyComponent();
        var tenant = new DummyTenant();
        var pool = new Mock<IComponentPool<DummyComponent, DummyComponentSpec>>();
        pool.Setup(x => x.AcquireComponentsAsync(It.IsAny<IReadOnlyDictionary<string, DummyComponentSpec>>()))
            .ReturnsAsync(new Dictionary<string, DummyComponent> { { "component", component } });

        var factory = new Mock<ITenantFactory<DummyTenant, DummyComponent, DummyTenantSpec>>();
        factory.Setup(x => x.AddTenantAsync(component, It.IsAny<DummyTenantSpec>())).ReturnsAsync(tenant);

        var dataIsland = new DataIslandBuilder()
            .AddComponentPool("component", pool.Object, factory.Object)
            .AddTemplate("template name", template =>
            {
                template.AddComponent<DummyComponent, DummyComponentSpec>("component");
                template.AddTenant<DummyTenant, DummyTenantSpec>("tenant", "component");
                template.AddDataAccess<DummyDataAccess>("tenant");
            })
            .Build(Mock.Of<ITestContext>());

        var a = await dataIsland.Materializer.MaterializeTenantsAsync("template name");

        var b = a.Single();
        Assert.Same(component, b.Component);
        Assert.Equal("component", b.ComponentName);
        Assert.Same(tenant, b.Instance);
        Assert.Equal("tenant", b.TenantName);
        Assert.Equal([typeof(DummyDataAccess)], b.DataAccess);
    }

    [Fact]
    public async Task Nonexistent_template_throws_exception()
    {
        var pool = new Mock<IComponentPool<DummyComponent, DummyComponentSpec>>();
        var factory = new Mock<ITenantFactory<DummyTenant, DummyComponent, DummyTenantSpec>>();
        var dataIsland = new DataIslandBuilder()
            .AddComponentPool("test", pool.Object, factory.Object)
            .Build(Mock.Of<ITestContext>());

        await Assert.ThrowsAsync<KeyNotFoundException>(() => dataIsland.Materializer.MaterializeTenantsAsync("nonexistent template"));
    }

    public class DummyComponent;
    public record DummyComponentSpec : ComponentSpec<DummyComponent>;
    public class DummyTenant;
    public record DummyTenantSpec : TenantSpec<DummyTenant>;
    public class DummyDataAccess;
}
