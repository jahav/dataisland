using DataIsland.xUnit.v3;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkedServer;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<LinkedUser> LinkedUsers { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LinkedUser>(entity =>
        {
            entity.ToTable("LinkedUsers");
            entity.HasNoKey();
        });
    }
}

public record LinkedUser(string Name);

public class LinkedDbContext(DbContextOptions<LinkedDbContext> options) : DbContext(options);

public class ClassFixture
{
    public ClassFixture(DataIslandFixture dataIslandFixture)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer());
        services.AddDbContext<LinkedDbContext>(opt => opt.UseSqlServer());

        services.AddDataIslandInProc<ClassFixture>(dataIslandFixture.Island);
        ServiceProvider = services.BuildServiceProvider();
    }

    public ServiceProvider ServiceProvider { get; }
}

[ApplyTemplate("default")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DummyTest(ClassFixture _fixture, ITestOutputHelper _output) : IClassFixture<ClassFixture>
{
    [Fact]
    public async Task Select_from_linked_table()
    {
        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            await using var linkedCtx = scope.ServiceProvider.GetRequiredService<LinkedDbContext>();
            var dbName = await linkedCtx.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync();
            _output.WriteLine($"Linked database: {dbName}");

            await linkedCtx.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE [dbo].[Users]([Name] NVARCHAR(10) PRIMARY KEY);
                INSERT INTO [dbo].[Users]([Name]) VALUES (N'Jane Doe'), (N'John Doe');
                """);
        }

        // Add a delay, so if the other test is failing, both fail
        await Task.Delay(3000);
        
        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            await using var appCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbName = await appCtx.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync();
            _output.WriteLine($"App database: {dbName}");

            var linkedUsers = await appCtx.LinkedUsers.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
            Assert.Equal(["Jane Doe", "John Doe"], linkedUsers);
        }
    }
}

[ApplyTemplate("default")]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DummyTest2(ClassFixture _fixture, ITestOutputHelper _output) : IClassFixture<ClassFixture>
{
    [Fact]
    public async Task Select_from_linked_table()
    {
        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            await using var linkedCtx = scope.ServiceProvider.GetRequiredService<LinkedDbContext>();
            var dbName = await linkedCtx.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync();
            _output.WriteLine($"Linked database: {dbName}");

            await linkedCtx.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE [dbo].[Users]([Name] NVARCHAR(10) PRIMARY KEY);
                INSERT INTO [dbo].[Users]([Name]) VALUES (N'Jane Doe'), (N'John Doe');
                """);
        }
        // Add a delay, so if the other test is failing, both fail
        await Task.Delay(3000);

        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            await using var appCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var dbName = await appCtx.Database.SqlQueryRaw<string>("SELECT DB_NAME() AS [Value]").SingleAsync();
            _output.WriteLine($"App database: {dbName}");

            var linkedUsers = await appCtx.LinkedUsers.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync();
            Assert.Equal(["Jane Doe", "John Doe"], linkedUsers);
        }
    }
}
