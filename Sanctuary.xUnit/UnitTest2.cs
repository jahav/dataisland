using Microsoft.Extensions.DependencyInjection;

namespace Sanctuary.xUnit;

[ScopedTenants]
[DataSetProfile]
public class UnitTest2 : IClassFixture<ClassFixture>
{
    private readonly ClassFixture _fixture;

    public UnitTest2(ClassFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Test1()
    {
        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            var test = scope.ServiceProvider.GetRequiredService<QueryDbContext>();
            var list = test.Users.Select(x => x.Name).OrderByDescending(x => x).ToList();
            Assert.Equal(new[] { "Dummy C", "Dummy B", "Dummy A", }, list);

            test.Users.Add(new Users()
            {
                UserId = Guid.NewGuid(),
                Name = "Test1",
            });
            test.SaveChanges();

            await Task.Delay(10000);

            var list1 = test.Users.Select(x => x.Name).OrderByDescending(x => x).ToList();
            Assert.Equal(new[] { "Test1", "Dummy C", "Dummy B", "Dummy A", }, list1);
        }
    }
}