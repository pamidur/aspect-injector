using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace AspectInjector.Tests.Build
{
    public class MsBuildIntegrationTests
    {
        /// <summary>
        /// Verifies real MSBuild integration across clean, incremental, and rebuild operations.
        /// </summary>
        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ConsoleApplicationBuildsRebuildsAndRuns(bool withAspect, bool asyncMethod)
        {
            var repository = FindRepository();
            var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent.Name;
            var taskAssembly = Path.Combine(repository, "src", "AspectInjector", "bin", configuration, "netstandard2.0", "AspectInjector.dll");
            var brokerAssembly = Path.Combine(Path.GetDirectoryName(taskAssembly), "AspectInjector.Broker.dll");
            var target = typeof(MsBuildIntegrationTests).Assembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
            var major = new FrameworkName(target).Version.Major;
            var framework = "net" + major + ".0";
            var directory = Path.Combine(Path.GetTempPath(), "AspectInjectorBuildTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                // Exercise the SDK matching this test target, rather than the parent SDK.
                File.WriteAllText(Path.Combine(directory, "global.json"), $@"{{""sdk"":{{""version"":""{major}.0.100"",""rollForward"":""latestMinor""}}}}");
                // Import the same targets as the NuGet package, with the locally built task.
                File.WriteAllText(Path.Combine(directory, "HelloWorld.csproj"), $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{framework}</TargetFramework>
    <AspectInjector_Location>{SecurityElement.Escape(taskAssembly)}</AspectInjector_Location>
    <AspectInjector_Verbose>true</AspectInjector_Verbose>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""AspectInjector.Broker""><HintPath>{SecurityElement.Escape(brokerAssembly)}</HintPath></Reference>
  </ItemGroup>
  <Import Project=""{SecurityElement.Escape(Path.Combine(repository, "src", "AspectInjector", "build", "AspectInjector.targets"))}"" />
</Project>");
                var program = withAspect ? AspectedProgram : PlainProgram;
                if (asyncMethod)
                    program = program.Replace("public static void Main() => Console.WriteLine(\"Hello World\");",
                        "public static async System.Threading.Tasks.Task Main() { await System.Threading.Tasks.Task.Yield(); Console.WriteLine(\"Hello World\"); }");
                File.WriteAllText(Path.Combine(directory, "Program.cs"), program);

                var first = await Dotnet(directory, "build", "-c", "Release", "-v:minimal");
                Assert.Contains(withAspect ? "Assembly has been patched." : "No patching required.", first);
                await Dotnet(directory, "build", "-c", "Release", "--no-restore", "-v:minimal");
                await Dotnet(directory, "build", "-c", "Release", "--no-restore", "-t:Rebuild", "-v:minimal");
                var output = await Dotnet(directory, Path.Combine(directory, "bin", "Release", framework, "HelloWorld.dll"));
                Assert.Equal(withAspect ? "Before\nHello World\nAfter" : "Hello World", output.Replace("\r\n", "\n").Trim());
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        /// <summary>
        /// Locates the repository root from the test output directory.
        /// </summary>
        private static string FindRepository()
        {
            for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
                if (File.Exists(Path.Combine(directory.FullName, "AspectInjector.sln")))
                    return directory.FullName;
            throw new DirectoryNotFoundException("Cannot locate AspectInjector.sln");
        }

        /// <summary>
        /// Runs a child .NET command with a timeout and requires successful completion.
        /// </summary>
        private static async Task<string> Dotnet(string directory, params string[] arguments)
        {
            var host = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
            if (string.IsNullOrEmpty(host))
            {
                var root = Environment.GetEnvironmentVariable("DOTNET_ROOT");
                host = string.IsNullOrEmpty(root) ? "dotnet" : Path.Combine(root, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
            }
            var start = new ProcessStartInfo(host)
            {
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            // The test runner inherits these from its SDK. Let global.json select the child SDK.
            start.Environment.Remove("MSBuildSDKsPath");
            start.Environment.Remove("MSBUILD_EXE_PATH");
            start.Environment.Remove("MSBuildExtensionsPath");
            foreach (var argument in arguments)
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start);
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            try
            {
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromMinutes(2));
            }
            catch
            {
                process.Kill(entireProcessTree: true);
                throw;
            }
            var combined = await output + await error;
            Assert.True(process.ExitCode == 0, $"dotnet {string.Join(" ", arguments)} failed:\n{combined}");
            return combined;
        }

        private const string PlainProgram = @"using System;
public class Program
{
    public static void Main() => Console.WriteLine(""Hello World"");
}";

        private const string AspectedProgram = @"using System;
using AspectInjector.Broker;
[Aspect(Scope.Global)]
[Injection(typeof(Trace))]
public class Trace : Attribute
{
    [Advice(Kind.Before)]
    public void Before() => Console.WriteLine(""Before"");
    [Advice(Kind.After)]
    public void After() => Console.WriteLine(""After"");
}
public class Program
{
    [Trace]
    public static void Main() => Console.WriteLine(""Hello World"");
}";
    }
}
