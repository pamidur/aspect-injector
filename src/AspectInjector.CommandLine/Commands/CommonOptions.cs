using CommandLine;

namespace AspectInjector.CLI.Commands
{
    public class CommonOptions
    {
        [Option('d', "debug", HelpText = "Launches debugger.")]
        public bool Debug { get; set; }
    }
}