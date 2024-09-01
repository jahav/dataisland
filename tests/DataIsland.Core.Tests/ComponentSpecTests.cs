namespace DataIsland.Core.Tests;

public class ComponentSpecTests
{
    [Fact]
    public void ComponentType_property_returns_generic_type()
    {
        var componentSpec = new TestComponentSpec();
        Assert.Equal(typeof(TestComponent), componentSpec.ComponentType);
    }

    private record TestComponentSpec : ComponentSpec<TestComponent>;

    private class TestComponent;
}
