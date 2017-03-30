using AspectInjector.CLI.Commands;
using CommandLine;

namespace AspectInjector.CLI
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ProcessCommand>(args)
                .MapResult(
                  (ProcessCommand cmd) => cmd.Execute(),
                  errs => 1);
        }
    }
}