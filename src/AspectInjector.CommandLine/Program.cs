using AspectInjector.CLI.Commands;
using CommandLine;
using System.Diagnostics;

namespace AspectInjector.CLI
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ProcessCommand, CommonOptions>(args)
                .WithParsed<CommonOptions>(co =>
                {
                    if (co.Debug) Debugger.Launch();
                })
                .MapResult(
                  (ProcessCommand cmd) => cmd.Execute(),
                  errs => 1);
        }
    }
}