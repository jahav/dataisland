namespace DataIsland.xUnit.v3;

internal class XUnitTestProvider : ICurrentTestProvider
{
    public string? CurrentTestId => Xunit.TestContext.Current.TestMethod?.UniqueID;
}
