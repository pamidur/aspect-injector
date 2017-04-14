using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspectInjector.Tests.General
{
    [TestClass]
    public class PETest
    {
        [TestMethod]
        public void General_PEIntegrity_IsOk()
        {
            var sdkFolder = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\Microsoft\\Microsoft SDKs\\NETFXSDK\\4.6", "InstallationFolder", null) as string;
            if (null == sdkFolder)
                throw new InvalidOperationException("Could not find Windows SDK v10.0A installation folder.");

            var peverify = Directory.GetFiles(sdkFolder, "peverify.exe", SearchOption.AllDirectories).First();

            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var proc = Process.Start(new ProcessStartInfo(peverify, assemblyPath) { UseShellExecute = false });

            proc.WaitForExit();

            Assert.AreEqual(0, proc.ExitCode);
        }
    }
}