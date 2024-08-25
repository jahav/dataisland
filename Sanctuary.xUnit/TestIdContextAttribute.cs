using Sanctuary.xUnit;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

namespace Sanctuary.xUnit;

public class TestIdContextAttribute : BeforeAfterTestAttribute
{
    public override ValueTask Before(MethodInfo methodUnderTest, IXunitTest test)
    {
        var ctx = Xunit.TestContext.Current;
        ctx.KeyValueStorage["TestId"] = methodUnderTest.Name;
        ctx.KeyValueStorage["ProfileName"] = "DefaultProfile";
        //File.WriteAllText(@"c:\temp\aaaaa.aa", "test");
        //TestContext.Instance.TestId = methodUnderTest.Name;
        //TestContext.Instance.ProfileName= ;
        return ValueTask.CompletedTask;
    }

    public override ValueTask After(MethodInfo methodUnderTest, IXunitTest test)
    {
        var ctx = Xunit.TestContext.Current;
        ctx.KeyValueStorage["TestId"] = null;
        ctx.KeyValueStorage["ProfileName"] = null;
        return ValueTask.CompletedTask;
    }
}