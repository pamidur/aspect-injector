using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Common;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core
{
    public class Processor : PatcherBase
    {
        private readonly IAspectReader _aspectExtractor;
        private readonly IAspectWeaver _aspectWeaver;
        private readonly IEnumerable<IEffectWeaver> _effectWeavers;
        private readonly IInjectionReader _injectionCollector;

        public Processor(
            IAspectReader aspectExtractor,
            IInjectionReader injectionCollector,
            IAspectWeaver aspectWeaver,
            IEnumerable<IEffectWeaver> effectWeavers,
            ILogger logger) : base(logger)
        {
            _aspectExtractor = aspectExtractor;
            _injectionCollector = injectionCollector;
            _aspectWeaver = aspectWeaver;
            _effectWeavers = effectWeavers;
        }

        protected override Rule GenericInfoRule => GeneralRules.Info;
        protected override Rule GenericErrorRule => GeneralRules.CompilationMustSecceedIfNoOtherErrors;

        protected override bool PatchAssembly(AssemblyDefinition assembly, bool optimize, bool verbose)
        {
            var aspects = _aspectExtractor.ReadAll(assembly);
            var injections = _injectionCollector.ReadAll(assembly).ToList();

            _log.Log(GeneralRules.Info, $"Found {aspects.Count} aspects, {injections.Count} injections");

            if (_log.IsErrorThrown)
                return false;

            var hasAspects = aspects.Count != 0;
            var hasInjections = injections.Count != 0;

            if (hasAspects)
            {
                if(verbose) _log.Log(GeneralRules.Info, "Processing aspects...");
                foreach (var aspect in aspects)
                    _aspectWeaver.WeaveGlobalAssests(aspect);
            }

            var modifiedBodies = new List<MethodBody>();
            CutEvents.OnModify = mb =>
            {
                if (!modifiedBodies.Contains(mb))
                {
                    mb.SimplifyMacros();
                    modifiedBodies.Add(mb);
                }
            };

            if (hasInjections)
            {
                if (verbose) _log.Log(GeneralRules.Info, "Processing injections...");

                foreach (var injector in _effectWeavers.OrderByDescending(i => i.Priority))
                {
                    if (verbose) _log.Log(GeneralRules.Info, $"Executing {injector.GetType().Name}...");

                    foreach (var prioritizedInjections in injections.GroupBy(i => i.Priority).OrderByDescending(a => a.Key).ToList())
                        foreach (var injection in prioritizedInjections.OrderByDescending(i => i.Effect.Priority))
                            if (injector.CanWeave(injection))
                            {
                                injector.Weave(injection);
                                injections.Remove(injection);
                            }
                }

                foreach (var injection in injections)
                    _log.Log(GeneralRules.UnexpectedCompilerBehaviour, injection.Target, $"Couldn't find weaver for => {injection.ToString()}");
            }

            if (hasAspects || hasInjections)
            {
                if (optimize)
                {
                    if (verbose) _log.Log(GeneralRules.Info, "Optimizing...");
                    foreach (var mb in modifiedBodies)
                        mb.OptimizeMacros();
                }

                if (verbose) _log.Log(GeneralRules.Info, "Processing is done.");
                return true;
            }

            return false;
        }
    }
}