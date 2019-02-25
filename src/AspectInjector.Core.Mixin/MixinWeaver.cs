using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using FluentIL.Logging;

namespace AspectInjector.Core.Mixin
{
    public class MixinWeaver : IEffectWeaver
    {
        private readonly ILogger _log;

        public MixinWeaver(ILogger log) => _log = log;

        public byte Priority => 10;

        public void Weave(InjectionDefinition injection)
        {
            var process = new MixinWeaveProcess(_log, injection.Target, injection.Source, (MixinEffect)injection.Effect);
            process.Execute();
        }

        public bool CanWeave(InjectionDefinition injection)
        {
            return injection.Effect is MixinEffect mixin && mixin.IsApplicableFor(injection.Target);
        }
    }
}