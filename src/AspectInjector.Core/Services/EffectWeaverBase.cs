using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Services
{
    public abstract class EffectWeaverBase : ServiceBase
    {
        public EffectWeaverBase(Logger logger) : base(logger)
        {
        }

        public byte Priority { get; protected set; }

        public abstract void Weave(Models.Injection injection);

        public abstract bool CanWeave(Models.Injection injection);
    }

    public abstract class EffectWeaverBase<TTarget, TEffect> : EffectWeaverBase
        where TTarget : ICustomAttributeProvider
        where TEffect : Effect
    {
        public EffectWeaverBase(Logger logger) : base(logger)
        {
        }

        public override void Weave(Models.Injection injection)
        {
            Weave((TTarget)injection.Target, (TEffect)injection.Effect, injection);
        }

        public override bool CanWeave(Models.Injection injection)
        {
            return injection.Target is TTarget && injection.Effect is TEffect;
        }

        protected abstract void Weave(TTarget target, TEffect effect, Models.Injection injection);
    }
}