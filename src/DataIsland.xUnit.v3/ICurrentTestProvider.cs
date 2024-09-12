namespace DataIsland.xUnit.v3;

/// <summary>
/// <para>
/// A provider that can return a unique identifier of currently running test.
/// </para>
/// <para>
/// This interface is needed, because we can test in two ways:
/// <list type="bullet">
///   <item>
///     <term>InProc</term>
///     <description>
///         Test are executed within a single async flow. Generally this means test resolves
///         dependency, calls some methods and everything is done directly from test method.
///         For this case, we use <see cref="Xunit.TestContext.Current"/> as an ambient
///         context used to keep track of currently running test.
///     </description>
///   </item>
///   <item>
///     <term>Separated</term>
///     <description>
///         The test calls some other component through network, generally through loopback.
///         Ultimately, everything is still running within the same process and all components
///         can share objects, but they are not connected through some kind of ambient context
///         that could keep track of currently executed test. We therefore need a way to get
///         identifier of currently running test. For ASP.NET Core, we sent it as HTTP header
///         through <see cref="TestIdHandler"/> and the <see cref="TestIdMiddleware"/> sets
///         it as current test for the processed request.
///     </description>
///   </item>
/// </list>
/// In both cases, the <see cref="XUnitTestContext"/> test id is necessary to get tenants created
/// for a test. Patchers resolve the <see cref="ITestContext"/> from MSDI and create patched
/// data access libraries.
/// </para>
/// </summary>
internal interface ICurrentTestProvider
{
    string? CurrentTestId { get; }
}
