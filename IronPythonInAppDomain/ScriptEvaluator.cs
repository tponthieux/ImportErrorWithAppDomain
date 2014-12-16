using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

[assembly: AllowPartiallyTrustedCallers]

namespace IronPythonInAppDomain
{
    public class ScriptEvaluator : MarshalByRefObject
    {
        public String PythonStandardLibaryPath { get; set; }
        public String PythonApplicationCodePath { get; set; }
        public ScriptEngine ScriptEngine { get; set; }
        public ScriptScope ScriptScope { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public MemoryStream StandardOut { get; set; }
        public MemoryStream StandardError { get; set; }

        public ScriptEvaluator()
        {
            PythonStandardLibaryPath = Path.GetFullPath(@"..\..\..\IronPythonEngine\standard_library");
            PythonApplicationCodePath = Path.GetFullPath(@"..\..\..\IronPythonEngine\python_code");
            StandardOut = new MemoryStream();
            StandardError = new MemoryStream();

        }

        public dynamic Get(String key)
        {
            return Variables[key];
        }

        public ScriptEvaluator InstantiateInAppDomain()
        {
            /* create app domain setup */
            var appDomainSetup = new AppDomainSetup();
            appDomainSetup.ApplicationBase = Environment.CurrentDirectory;
            //appDomainSetup.ApplicationBase = Path.GetDirectoryName(Environment.CurrentDirectory);
            appDomainSetup.DisallowBindingRedirects = false;
            appDomainSetup.DisallowCodeDownload = true;
            //appDomainSetup.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            /* assign permissions */
            //var permissionSet = new PermissionSet(PermissionState.Unrestricted);
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, PythonStandardLibaryPath));
            permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, PythonApplicationCodePath));

            permissionSet.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new FileDialogPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new GacIdentityPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new IsolatedStorageFilePermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new KeyContainerPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new PrincipalPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new PublisherIdentityPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new RegistryPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new SiteIdentityPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new StorePermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new StrongNameIdentityPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new TypeDescriptorPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new UIPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new UrlIdentityPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new ZoneIdentityPermission(PermissionState.Unrestricted));
            permissionSet.AddPermission(new SecurityPermission(PermissionState.Unrestricted));

            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Assertion));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.BindingRedirects));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlAppDomain));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlDomainPolicy));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlEvidence));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlPolicy));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlPrincipal));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.ControlThread));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Infrastructure));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.NoFlags));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.RemotingConfiguration));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.SkipVerification));
            //permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));

            /* create an array of fully trusted assemblies */
            var fullTrustAssemblies = new List<StrongName>();
            //fullTrustAssemblies.Add(typeof(ScriptEvaluator).Assembly.Evidence.GetHostEvidence<StrongName>());
            //fullTrustAssemblies.Add(typeof(ScriptEngine).Assembly.Evidence.GetHostEvidence<StrongName>());
            //fullTrustAssemblies.Add(typeof(ScriptRuntime).Assembly.Evidence.GetHostEvidence<StrongName>());
            //fullTrustAssemblies.Add(typeof(ScriptScope).Assembly.Evidence.GetHostEvidence<StrongName>());
            fullTrustAssemblies.Add(typeof(Dictionary<string, object>).Assembly.Evidence.GetHostEvidence<StrongName>());

            /* some permission is missing for debugging, so use this as a workaround */
            if (Debugger.IsAttached)
            {
                //permissionSet = new PermissionSet(PermissionState.Unrestricted);
                permissionSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            }

            /* create a script evaluator instance in the app domain and return a reference */
            var appDomain = AppDomain.CreateDomain("Sandbox", null, appDomainSetup, permissionSet, fullTrustAssemblies.ToArray());
            var scriptEvaluator = (ScriptEvaluator)appDomain.CreateInstanceAndUnwrap("IronPythonInAppDomain", typeof(ScriptEvaluator).FullName);

            scriptEvaluator.CreateScriptEngine();
            return scriptEvaluator;
        }

        public ScriptEngine CreateScriptEngine()
        {
            /* create the iron IronPython engine */
            var options = new Dictionary<string, object> { { "Debug", Debugger.IsAttached } };
            //options["ExceptionDetail"] = true;
            //options["ClrDebuggingEnabled"] = true;
            ScriptEngine = Python.CreateEngine(options);

            /* add library directories the search paths */
            var searchPaths = ScriptEngine.GetSearchPaths();
            searchPaths.Add(PythonStandardLibaryPath);
            searchPaths.Add(PythonApplicationCodePath);
            ScriptEngine.SetSearchPaths(searchPaths);

            /* set output streams */
            ScriptEngine.Runtime.IO.SetOutput(StandardOut, Encoding.UTF8);
            ScriptEngine.Runtime.IO.SetErrorOutput(StandardError, Encoding.UTF8);

            return ScriptEngine;
        }

        public ScriptScope CreateScope()
        {
            ScriptScope = ScriptEngine.CreateScope();
            return ScriptScope;
        }

        public ScriptSource CreateScriptSource(String script)
        {
            return ScriptEngine.CreateScriptSourceFromString(script, SourceCodeKind.Statements);
        }
    }
}

//Stopwatch stopwatch = Stopwatch.StartNew();
//stopwatch.Stop(); Console.WriteLine("Time taken: {0}ms", stopwatch.Elapsed.TotalMilliseconds);