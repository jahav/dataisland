namespace DataIsland.Core.Tests;

public class AdoNetTenantTests
{
    [Fact]
    public void Class_is_abstract()
    {
        Assert.True(typeof(AdoNetTenant).IsAbstract);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    public void Connection_string_is_required(string? emptyConnectionString)
    {
        var ex = Assert.Throws<ArgumentException>(() => new TestAdoNetTenant(emptyConnectionString!, "component"));
        Assert.NotNull(ex);
        Assert.Equal("connectionString", ex.ParamName);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    public void Component_name_is_required(string? emptyComponentName)
    {
        var ex = Assert.Throws<ArgumentException>(() => new TestAdoNetTenant("Data Source=.", emptyComponentName!));
        Assert.NotNull(ex);
        Assert.Equal("databaseName", ex.ParamName);
    }

    [Fact]
    public void Allows_invalid_connection_strings()
    {
        const string invalidConnectionString = "nonsensical string; = definitely not valid connection string";
        var tenant = new TestAdoNetTenant(invalidConnectionString, "component");
        Assert.Equal(invalidConnectionString, tenant.ConnectionString);
    }

    private record TestAdoNetTenant(string ConnectionString, string DatabaseName)
        : AdoNetTenant(ConnectionString, DatabaseName);
}
