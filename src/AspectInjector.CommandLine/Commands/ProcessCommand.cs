using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services;
using AspectInjector.Core.Utils;
using CommandLine;
using DryIoc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AspectInjector.CLI.Commands
{
    [Verb("process", HelpText = "Instructs Aspect Injector to process injections")]
    public class ProcessCommand : CommandBase
    {
        [Option('f', "file", Required = true, HelpText = ".net assembly file for processing (typically exe or dll)")]
        public string Filename { get; set; }

        [Option('r', "references", HelpText = "Referenced assemblies semicolon separated")]
        public string References { get; set; }

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

            var resolver = new KnownReferencesAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(Filename));
            if (References != null)
                References.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(resolver.AddReference);

            try
            {
                processor.Process(Filename, resolver);
            }
            catch (Exception e)
            {
                Log.LogError(CompilationMessage.From(e.ToString()));
            }

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

            //register effects

            container.Register<IEffectExtractor, MixinExtractor>();
            container.Register<IEffectExtractor, AdviceExtractor>();

            container.Register<IEffectWeaver, MixinWeaver>();
            container.Register<IEffectWeaver, AdviceInlineWeaver>();
            container.Register<IEffectWeaver, AdviceAroundWeaver>();
            container.Register<IEffectWeaver, AdviceStateMachineWeaver>();

            //done registration

            return container.Resolve<Processor>();
        }
    }
}