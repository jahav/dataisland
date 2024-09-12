using System.Threading;

namespace DataIsland.xUnit.v3;

/// <summary>
/// A provider of current test for ASP.NET integration tests. The <see cref="TestIdMiddleware"/>
/// sets and clears <see cref="CurrentTestId"/>. It is registered as a singleton to MSDI container
/// of tested component.
/// </summary>
internal class AspNetTestProvider : ICurrentTestProvider
{
    // TODO: This is ugly, but I don't know how to fix it. Provider is built in Build() method, but
    // I need to access in TestIdMiddleware. Remove ITestContext from DataIsland and just create it
    // in Build*() methods.
    private static readonly AsyncLocal<string?> TestId = new();

    public string? CurrentTestId => TestId.Value;

    internal void SetTestId(string? value)
    {
        TestId.Value = value;
    }
}
