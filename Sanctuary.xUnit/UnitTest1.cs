using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sanctuary.EfCore;

namespace Sanctuary.xUnit
{
    //public class UnitTest1
    //{
    //    [Fact]
    //    public void Test1()
    //    {
    //        var services = new ServiceCollection();
    //        services.AddDbContext<TestDbContext>(a => a.EnableDetailedErrors().UseSqlServer("abc"));

    //        // Replace accessors
    //        var efCoreAccessor = new EfCoreAccessor<TestDbContext>();
    //        efCoreAccessor.Register(services);

    //        var sp = services.BuildServiceProvider();

    //        TestContext._testId.Value = "UnitTest1.Test1";

    //        var dbContext = sp.GetRequiredService<TestDbContext>();

    //        var cc = dbContext.Database.CanConnect();

    //        TestContext._testId.Value = null;
    //    }
    //}
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class DataSetDefinitionAttribute : Attribute
{
    public DataSetDefinitionAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Name of a data set collection.
    /// </summary>
    public string Name { get; }
}


// DataStore - what If I have multiple data stores.

[DataSetDefinition("one")]
public class DataSetOne
{
    public void Resolve(IServiceProvider serviceProvider)
    {

    }
}

public class RelationalDatabaseComponent
{
    public string ConnectionString => throw new NotImplementedException();
}

internal class TestContext
{
    internal static AsyncLocal<string> _testId = new AsyncLocal<string>();

    public static string TestId => _testId.Value;
}