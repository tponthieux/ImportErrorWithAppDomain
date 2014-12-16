using NUnit.Framework;

namespace IronPythonInAppDomain
{
    [TestFixture]
    public class ScriptEvaluatorTest
    {
        [Test]
        public void ExecuteWithOsModuleTest()
        {
            Program.ExecuteWithOsModule();
        }
    }
}

