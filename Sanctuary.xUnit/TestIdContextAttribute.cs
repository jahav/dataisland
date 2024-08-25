using System.Reflection;
using Xunit.Sdk;

namespace Sanctuary.xUnit;

public class TestIdContextAttribute : BeforeAfterTestAttribute
{
    public override void Before(MethodInfo methodUnderTest)
    {
        TestContext.Instance.TestId = methodUnderTest.Name;
        TestContext.Instance.ProfileName= "DefaultProfile";
    }

    public override void After(MethodInfo methodUnderTest)
    {
        TestContext.Instance.TestId = null;
        TestContext.Instance.ProfileName = null;
    }
}