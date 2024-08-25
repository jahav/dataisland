namespace Sanctuary.xUnit;

[CollectionDefinition(Name)]
public class TestCollection : ICollectionFixture<IocFixture>
{
    public const string Name = nameof(TestCollection);
}