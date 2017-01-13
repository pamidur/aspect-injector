using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Services.Injection
{
    public abstract class WeaverBase : ServiceBase
    {
        public WeaverBase(Logger logger) : base(logger)
        {
        }

        public byte Priority { get; protected set; }

        public abstract void Weave(Models.Injection injection);

        public abstract bool CanWeave(Models.Injection injection);
    }

    public abstract class WeaverBase<TTarget, TEffect> : WeaverBase
        where TTarget : ICustomAttributeProvider
        where TEffect : Effect
    {
        public WeaverBase(Logger logger) : base(logger)
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