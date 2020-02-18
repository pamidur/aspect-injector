using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Services;
using AspectInjector.Rules;
using FluentIL.Logging;
using System;
using System.Collections.Generic;

namespace AspectInjector
{
    public class Compiler
    {
        public int Execute(string filename, IReadOnlyList<string> references, bool optimize, bool verbose)
        {
#pragma warning disable S1854 // Unused assignments should be removed
            var version = typeof(Compiler).Assembly.GetName().Version.ToString(3);
#pragma warning restore S1854 // Unused assignments should be removed

#if DEBUG
            version = "DEV";
#endif

            var app = $"AspectInjector|{version}";
            var log = new ConsoleLogger(app);

            try
            {
                var processor = CreateApp(log);
                processor.Process(filename, references, optimize, verbose);
            }
            catch (Exception e)
            {
                log.Log(GeneralRules.CompilationMustSecceedIfNoOtherErrors, $"Processing failure: {e.ToString()}");
            }

            return log.IsErrorThrown ? 1 : 0;
        }

        private Processor CreateApp(ILogger logger)
        {
            var aspectReader = new AspectReader(new IEffectReader[] {
                new MixinReader(),
                new AdviceReader(logger)
            }, logger);
            var processor = new Processor(aspectReader, new InjectionReader(aspectReader, logger), new AspectWeaver(logger), new IEffectWeaver[] {
                new MixinWeaver(logger),
                new AdviceInlineWeaver(logger),
                new AdviceAroundWeaver(logger),
                new AdviceStateMachineWeaver(logger)
            }, logger);

            return processor;
        }
    }
}