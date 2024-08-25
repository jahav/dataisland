using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Core;

namespace Sanctuary.xUnit;

public class UnitTest3
{
    private readonly IocFixture _fixture;

    public UnitTest3(IocFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [DataSetProfile]
    public async Task Test2()
    {
        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            var test = scope.ServiceProvider.GetRequiredService<TestDbContext>();
            var list = test.Users.Select(x => x.Name).OrderBy(x => x).ToList();
            Assert.Equal(new[] { "Dummy A", "Dummy B", "Dummy C", }, list);
            test.Users.Add(new Users()
            {
                UserId = Guid.NewGuid(),
                Name = "Test2",
            });
            test.SaveChanges();

            await Task.Delay(10000);

            var list1 = test.Users.Select(x => x.Name).OrderBy(x => x).ToList();
            Assert.Equal(new[] { "Dummy A", "Dummy B", "Dummy C", "Test2" }, list1);
        }
    }
}