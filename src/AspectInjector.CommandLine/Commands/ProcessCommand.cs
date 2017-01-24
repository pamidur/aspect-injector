using AspectInjector.Core;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services;
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
                Log.LogError(CompilationMessage.From($"File {Filename} does not exist."));
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

        private Processor CreateProcessor()
        {
            var cache = new AssetsCache();

            return new Processor(
                new Janitor(null, Log),
                new AspectExtractor(new[] {
                    new MixinReader
                }, Log),
                cache,
                new InjectionCollector(cache, Log),
                new AspectWeaver(Log),
                new[] {
                },
                Log
                );
        }
    }
}