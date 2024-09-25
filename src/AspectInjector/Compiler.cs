using AspectInjector.Core;
using AspectInjector.Core.Advice;
using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Mixin;
using AspectInjector.Core.Services;
using AspectInjector.Rules;
using FluentIL.Logging;
using Microsoft.Build.Utilities;

namespace AspectInjector;

public class Compiler
{
    public bool Execute(string filename, IReadOnlyList<string> references, bool optimize, bool verbose, TaskLoggingHelper logHelper)
    {
#pragma warning disable S1854 // Unused assignments should be removed
        var version = typeof(Compiler).Assembly.GetName().Version.ToString(3);
#pragma warning restore S1854 // Unused assignments should be removed

#if DEBUG
        version = "DEV";
#endif

        var app = $"AspectInjector|{version}";
        var log = new MsBuildLogger(logHelper, verbose);
        log.Log(GeneralRules.Info, app);

        try
        {
            var processor = CreateApp(log);
            processor.Process(filename, references, optimize, verbose);
        }
        catch (Exception e)
        {
            log.Log(GeneralRules.CompilationMustSecceedIfNoOtherErrors, $"Processing failure: {e}");
        }

        return !log.IsErrorThrown;
    }

    private Processor CreateApp(ILogger logger)
    {
        var aspectReader = new AspectReader([
            new MixinReader(),
            new AdviceReader(logger)
        ], logger);
        var processor = new Processor(aspectReader, new InjectionReader(aspectReader, logger), new AspectWeaver(logger), [
            new MixinWeaver(logger),
            new AdviceInlineWeaver(logger),
            new AdviceAroundWeaver(logger),
            new AdviceStateMachineWeaver(logger)
        ], logger);

        return processor;
    }
}