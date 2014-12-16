using System;
using Microsoft.Scripting;

namespace IronPythonInAppDomain
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ExecuteWithOsModule();
            Console.ReadKey();
        }

        public static void ExecuteWithOsModule()
        {
            Console.WriteLine("\n** " + System.Reflection.MethodBase.GetCurrentMethod().Name + " **\n");
            /* create the AppDomain */
            var scriptEvaluator = new ScriptEvaluator().InstantiateInAppDomain();

            /* create script scope */
            var scriptScope = scriptEvaluator.CreateScope();

            /* create script source */
            var script = "import os";
            var scriptSource = scriptEvaluator.ScriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.Statements);

            /* execute script */
            scriptSource.Execute(scriptScope);
        }
    }
}

