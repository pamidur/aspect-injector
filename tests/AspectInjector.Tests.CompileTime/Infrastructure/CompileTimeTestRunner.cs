using AspectInjector.Broker;
using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Services;
using DryIoc;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspectInjector.CompileTimeTests
{
    public abstract class CompileTimeTestRunner
    {
        private AssemblyDefinition _asm;

        [TestInitialize]
        public void Init()
        {
            var type = GetType();

            var tag = new TestAssemblyGenerator(type);

            _asm = tag.CreateTestAssembly();

            var asmproc = CreateProcessor();

            asmproc.ProcessAssembly(_asm);
        }

        [TestMethod]
        protected void PE_Integrity_Is_Ok()
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

        private Processor CreateProcessor()
        {
            var container = new Container();

            //register main services

            container.Register<Processor>(Reuse.Singleton);
            container.Register<IAspectExtractor, AspectExtractor>(Reuse.Singleton);
            container.Register<IAspectWeaver, AspectWeaver>(Reuse.Singleton);
            container.Register<IAssetsCache, AssetsCache>(Reuse.Singleton);
            container.Register<IInjectionCollector, InjectionCollector>(Reuse.Singleton);
            container.Register<IJanitor, Janitor>(Reuse.Singleton);
            container.Register<ILogger, Core.Services.Logger>(Reuse.Singleton);

            //register weavers

            container.Register<IEffectExtractor, MixinExtractor>();
            container.Register<IEffectExtractor, AdviceExtractor>();

            container.Register<IEffectWeaver, MixinWeaver>();
            container.Register<IEffectWeaver, AdviceInlineWeaver>();
            container.Register<IEffectWeaver, AdviceAroundWeaver>();
            container.Register<IEffectWeaver, AdviceStateMachineWeaver>();

            //done registration

            return container.Resolve<Processor>();
        }
    }
}