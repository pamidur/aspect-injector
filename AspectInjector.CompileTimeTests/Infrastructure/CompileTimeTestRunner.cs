﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using AspectInjector.BuildTask;
using AspectInjector.BuildTask.Processors;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;

namespace AspectInjector.CompileTimeTests.Infrastructure
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

            var asmproc = new AssemblyProcessor(Configuration.GetProcessorsTree());

            asmproc.Process(_asm);
        }

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
    }
}