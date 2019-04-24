using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Services;
using AspectInjector.Rules;
using DryIoc;
using FluentIL.Logging;
using System;
using System.Collections.Generic;

namespace AspectInjector
{
    public class Compiler
    {
        public int Execute(string filename, IReadOnlyList<string> references, bool optimize, bool verbose)
        {
            var container = GetAppContainer();
            var log = container.Resolve<ILogger>();

            try
            {  
                var processor = container.Resolve<Processor>();
                processor.Process(filename, references, optimize, verbose);
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
            container.RegisterDelegate<ILogger>(c => new ConsoleLogger("AspectInjector"), Reuse.Singleton);

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