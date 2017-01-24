using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Services
{
    public abstract class EffectWeaverBase<TTarget, TEffect> : IEffectWeaver
        where TTarget : ICustomAttributeProvider
        where TEffect : Effect
    {
        protected readonly ILogger _log;

        public EffectWeaverBase(ILogger logger)
        {
            _log = logger;
        }

        public byte Priority { get; protected set; }

        public void Weave(Injection injection)
        {
            if (CanWeave(injection))
                Weave((TTarget)injection.Target, (TEffect)injection.Effect, injection);
        }

        protected virtual bool CanWeave(Injection injection)
        {
            return injection.Target is TTarget && injection.Effect is TEffect;
        }

        protected abstract void Weave(TTarget target, TEffect effect, Injection injection);
    }
}