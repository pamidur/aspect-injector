using AspectInjector.Core;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services;
using AspectInjector.Core.Utils;
using CommandLine;
using DryIoc;
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

            var processor = CreateProcessor();

            var resolver = new CachedAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(Filename));

            processor.Process(Filename, resolver);

            return Log.IsErrorThrown ? 1 : 0;
        }

        private Processor CreateProcessor()
        {
            var container = new Container();
            container.Register<IAspectExtractor, AspectExtractor>(Reuse.Singleton);
            container.Register<IAspectWeaver, AspectWeaver>(Reuse.Singleton);
            container.Register<IAssetsCache, AssetsCache>(Reuse.Singleton);
            container.Register<IInjectionCollector, InjectionCollector>(Reuse.Singleton);
            container.Register<IJanitor, Janitor>(Reuse.Singleton);

            //register weavers

            container.UseInstance(Log, true);

            return container.Resolve<Processor>();
        }
    }
}