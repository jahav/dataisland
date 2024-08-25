using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Core;
using Sanctuary.xUnit;

#pragma warning disable xUnit1051

[assembly: AssemblyFixture(typeof(IocFixture))]

namespace Sanctuary.xUnit;

//[Collection(TestCollection.Name)]
[TestIdContext]
public class UnitTest2
{
    private readonly IocFixture _fixture;

    public UnitTest2(IocFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [DataSetProfile]
    public async Task Test1()
    {
        using (var scope = _fixture.ServiceProvider.CreateScope())
        {
            var test = scope.ServiceProvider.GetRequiredService<TestDbContext>();
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