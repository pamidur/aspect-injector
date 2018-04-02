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
            var sdksNodes = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Microsoft SDKs\\NETFXSDK\\");

            var latestSdk = sdksNodes.GetSubKeyNames().OrderByDescending(s => s).FirstOrDefault();

            if(latestSdk == null)
                throw new InvalidOperationException("Could not find Windows SDK.");            

            var sdkFolder = sdksNodes.OpenSubKey(latestSdk).GetValue("InstallationFolder", null) as string;

            if (sdkFolder == null)
                throw new InvalidOperationException($"SDK folder is not found for net {latestSdk}.");

            var peverify = Directory.GetFiles(sdkFolder, "peverify.exe", SearchOption.AllDirectories).First();

            var assemblyPath = Assembly.GetExecutingAssembly().Location;

            var proc = Process.Start(new ProcessStartInfo(peverify, assemblyPath) { UseShellExecute = false });

            proc.WaitForExit();

            Assert.AreEqual(0, proc.ExitCode);
        }
    }
}