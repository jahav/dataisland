namespace Sanctuary;

public interface ITestComponentProvider<TDataComponent>
{
    /// <summary>
    /// Get component for current
    /// </summary>
    /// <returns></returns>
    TDataComponent GetComponent(string testId);
}