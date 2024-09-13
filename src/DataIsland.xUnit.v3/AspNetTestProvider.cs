using System.Threading;

namespace DataIsland.xUnit.v3;

/// <summary>
/// A provider of current test for ASP.NET integration tests. The <see cref="TestIdMiddleware"/>
/// sets and clears <see cref="CurrentTestId"/>. It is registered as a singleton to MSDI container
/// of tested component.
/// </summary>
internal class AspNetTestProvider : ICurrentTestProvider
{
    private readonly AsyncLocal<string?> _testId = new();

    public string? CurrentTestId => _testId.Value;

    internal void SetTestId(string? value)
    {
        _testId.Value = value;
    }
}
