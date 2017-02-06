using AspectInjector.Core;
using AspectInjector.Core.Advice;
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
    public class ProcessCommand : CommandBase
    {
        [Option('f', "file", Required = true, HelpText = ".net assembly file for processing (typically exe or dll)")]
        public string Filename { get; set; }

        public override int Execute()
        {
            var result = base.Execute();
            if (result != 0)
                return result;

            if (!File.Exists(Filename))
            {
                Log.LogError(CompilationMessage.From($"File {Filename} does not exist."));
                return 1;
            }

            var processor = CreateProcessor();

            var resolver = new CachedAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(Filename));

            processor.Process(Filename, resolver);

            return Log.IsErrorThrown ? 1 : result;
        }

        private Processor CreateProcessor()
        {
            var container = new Container();

            //register main services

            container.Register<Processor>(Reuse.Singleton);
            container.Register<IAspectExtractor, AspectExtractor>(Reuse.Singleton);
            container.Register<IAspectWeaver, AspectWeaver>(Reuse.Singleton);
            container.Register<IAssetsCache, AssetsCache>(Reuse.Singleton);
            container.Register<IInjectionCollector, InjectionCollector>(Reuse.Singleton);
            container.Register<IJanitor, Janitor>(Reuse.Singleton);
            container.UseInstance(Log, true);

            //register weavers

            container.Register<IEffectExtractor, MixinExtractor>(Reuse.Singleton);
            container.Register<IEffectExtractor, AdviceExtractor>(Reuse.Singleton);
            container.Register<IEffectWeaver, MixinWeaver>(Reuse.Singleton);

            //done registration

            return container.Resolve<Processor>();
        }
    }
}