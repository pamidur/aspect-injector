using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Services;
using AspectInjector.Core.Utils;
using AspectInjector.Rules;
using DryIoc;
using System;
using System.IO;

namespace AspectInjector
{
    public class Compiler
    {
        public int Execute(string filename, ArraySegment<string> references, bool optimize)
        {
            var container = GetAppContainer();
            var log = container.Resolve<ILogger>();

            try
            {
                if (!File.Exists(filename)) throw new FileNotFoundException("Target not found", filename);

                var resolver = new KnownReferencesAssemblyResolver();
                resolver.AddSearchDirectory(Path.GetDirectoryName(filename));

                foreach (var refr in references)
                {
                    if (!File.Exists(refr)) throw new FileNotFoundException("Reference not found", filename);
                    resolver.AddReference(refr);
                }

                var processor = container.Resolve<Processor>();
                processor.Process(filename, resolver, optimize);
            }
            catch (FileNotFoundException e)
            {
                log.Log(GeneralRules.CompilationMustSecceedIfNoOtherErrors, $"{e.Message}: '{e.FileName}'");
            }
            catch (Exception e)
            {
                log.Log(GeneralRules.CompilationMustSecceedIfNoOtherErrors, $"Processing failure: {e.ToString()}");
            }

            return log.IsErrorThrown ? 1 : 0;
        }
        private Container GetAppContainer()
        {
            var container = new Container();

            //register main services

            container.Register<Processor>(Reuse.Singleton);
            container.Register<IAspectReader, AspectReader>(Reuse.Singleton);
            container.Register<IAspectWeaver, AspectWeaver>(Reuse.Singleton);
            container.Register<IInjectionReader, InjectionReader>(Reuse.Singleton);
            container.Register<ILogger, Logger>(Reuse.Singleton);

            //register effects

            container.Register<IEffectReader, MixinReader>();
            container.Register<IEffectReader, AdviceReader>();

            container.Register<IEffectWeaver, MixinWeaver>();
            container.Register<IEffectWeaver, AdviceInlineWeaver>();
            container.Register<IEffectWeaver, AdviceAroundWeaver>();
            container.Register<IEffectWeaver, AdviceStateMachineWeaver>();

            //done registration

            return container;
        }
    }
}