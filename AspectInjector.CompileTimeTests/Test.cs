using AspectInjector.Broker;
using AspectInjector.BuildTask;
using AspectInjector.BuildTask.Processors;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspectInjector.CompileTimeTests
{
    public class CompileTimeTestRunner
    {
        private AssemblyDefinition _asm;

        [TestInitialize]
        public void Init()
        {
            var type = GetType();

            var tag = new TestAssemblyGenerator(type);

            _asm = tag.CreateTestAssembly();

            var asmproc = new AssemblyProcessor(Configuration.GetProcessorsTree());

            //asmproc.Process(_asm);
        }

        [TestMethod]
        public void PE_Integrity_Is_Ok()
        {
            var tempFile = Path.GetTempFileName() + ".dll";
            _asm.Write(tempFile);

            var sdkFolder = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.VersionLatest, VisualStudioVersion.VersionLatest);

            var peverify = Directory.GetFiles(sdkFolder, "peverify.exe", SearchOption.AllDirectories).First();

            var proc = Process.Start(new ProcessStartInfo(peverify, tempFile) { UseShellExecute = false });

            proc.WaitForExit();

            Assert.AreEqual(0, proc.ExitCode);

            File.Delete(tempFile);
        }
    }

    [TestClass]
    public class NonStaticMembersTests : CompileTimeTestRunner
    {
        [TestMethod]
        public void TestSomething()
        {
        }

        public class TargetClass
        {
            [Aspect(typeof(AspectImplementation))]
            public void TestMethod()
            {
            }
        }

        public class AspectImplementation
        {
            [Advice(InjectionPoints.Before, InjectionTargets.Method)]
            public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Instance)] object instance)
            {
            }
        }
    }

    //[TestClass]
    //public class NonStaticMembersTests
    //{
    //    [TestMethod]
    //    public void AdviceArguments_Instance_Not_Null_on_NonStaticMethod()
    //    {
    //        Checker.Passed = false;
    //        new TargetClass().TestMethod();
    //        Assert.IsTrue(Checker.Passed);
    //    }

    //    internal class TargetClass
    //    {
    //        [Aspect(typeof(AspectImplementation))]
    //        public void TestMethod()
    //        {
    //        }
    //    }

    //    internal class AspectImplementation
    //    {
    //        [Advice(InjectionPoints.Before, InjectionTargets.Method)]
    //        public void BeforeMethod([AdviceArgument(AdviceArgumentSource.Instance)] object instance)
    //        {
    //            Checker.Passed = instance != null;
    //        }
    //    }
    //}
}