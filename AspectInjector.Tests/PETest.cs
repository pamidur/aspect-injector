using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspectInjector.Tests
{
    [TestClass]
    public class PETest
    {
        [TestMethod]
        public void PE_Integrity_Is_Ok()
        {
            var sdkFolder = ToolLocationHelper.GetPathToDotNetFrameworkSdk(TargetDotNetFrameworkVersion.VersionLatest, VisualStudioVersion.VersionLatest);

            var peverify = Directory.GetFiles(sdkFolder, "peverify.exe", SearchOption.AllDirectories).First();

            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var proc = Process.Start(new ProcessStartInfo(peverify, assemblyPath) { UseShellExecute = false });

            proc.WaitForExit();

            Assert.AreEqual(0, proc.ExitCode);
        }
    }
}