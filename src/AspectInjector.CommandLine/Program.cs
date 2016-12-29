using AspectInjector.CLI.Commands;
using CommandLine;
using System.Diagnostics;

namespace AspectInjector.CLI
{
    internal class Program
    {
        private static int Main(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif

            return Parser.Default.ParseArguments<ProcessCommand>(args)
                .MapResult(
                  (ProcessCommand cmd) => cmd.Execute(),
                  errs => 1);
        }
    }
}