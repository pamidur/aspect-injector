using AspectInjector.Core;
using AspectInjector.Core.Configuration;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Utils;
using CommandLine;
using System.IO;

namespace AspectInjector.CLI.Commands
{
    [Verb("process", HelpText = "Instructs Aspect Injector to process injections")]
    public class ProcessCommand
    {
        [Option('f', "file", Required = true, HelpText = ".net assembly file for processing (typically exe or dll)")]
        public string Filename { get; set; }

        private ILogger Log { get; } = new ConsoleLogger();

        public int Execute()
        {
            if (!File.Exists(Filename))
            {
                Log.LogError($"File {Filename} does not exists.");
                return 1;
            }

            var processor = new Processor(
                ProcessingConfiguration.Default
                .SetLogger(Log)
                .UseMixinInjections()
                );

            var resolver = new CachedAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(Filename));

            processor.Process(Filename, resolver);

            return Log.IsErrorThrown ? 1 : 0;
        }
    }
}