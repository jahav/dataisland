using Microsoft.Extensions.DependencyInjection;
using Sanctuary.Core;

namespace Sanctuary.xUnit;

[Collection(TestCollection.Name)]
public class UnitTest2
{
    private readonly IocFixture _fixture;

    public UnitTest2(IocFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    [DataSetProfile]
    [TestIdContext]
    public void Test1()
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

            var list1 = test.Users.Select(x => x.Name).OrderByDescending(x => x).ToList();
            Assert.Equal(new[] { "Test1", "Dummy C", "Dummy B", "Dummy A", }, list1);
        }
    }

    [Fact]
    [DataSetProfile]
    [TestIdContext]
    public void Test2()
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
            var list1 = test.Users.Select(x => x.Name).OrderBy(x => x).ToList();
            Assert.Equal(new[] { "Dummy A", "Dummy B", "Dummy C", "Test2"}, list1);
        }
    }
}