using AspectInjector.Core.Contracts;
using CommandLine;
using System.Diagnostics;

namespace AspectInjector.CLI.Commands
{
    public class CommandBase
    {
        [Option('d', "debug", HelpText = "Launches debugger.")]
        public bool Debug { get; set; }

        protected ILogger Log { get; } = new ConsoleLogger();

        public virtual int Execute()
        {
            if (Debug) Debugger.Launch();
            return 0;
        }
    }
}