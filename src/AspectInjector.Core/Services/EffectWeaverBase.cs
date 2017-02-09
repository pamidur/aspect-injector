using AspectInjector.Core.Contracts;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Services
{
    public abstract class EffectWeaverBase<TTarget, TEffect> : IEffectWeaver
        where TTarget : IMemberDefinition
        where TEffect : Effect
    {
        protected readonly ILogger _log;

        public class ResolvedInjection : Injection
        {
            private ExtendedTypeSystem _ts;
            public new TEffect Effect { get { return (TEffect)base.Effect; } }
            public new TTarget Target { get { return (TTarget)base.Target; } }

            public ExtendedTypeSystem TypeSystem
            {
                get
                {
                    if (_ts == null)
                    {
                        if (Target is TypeDefinition)
                            _ts = ((TypeDefinition)base.Target).Module.GetTypeSystem();
                        else
                            _ts = base.Target.DeclaringType.Module.GetTypeSystem();
                    }
                    return _ts;
                }
            }
        }

        public EffectWeaverBase(ILogger logger)
        {
            _log = logger;
        }

        public byte Priority { get; protected set; }

        public void Weave(Injection injection)
        {
            if (CanWeave(injection))
                Weave(GetResolvedInjection(injection));
        }

        protected virtual bool CanWeave(Injection injection)
        {
            return injection.Target is TTarget && injection.Effect is TEffect;
        }

        protected abstract void Weave(ResolvedInjection injection);

        private ResolvedInjection GetResolvedInjection(Injection originalInjection)
        {
            Injection newInj = new ResolvedInjection();

            newInj.Effect = originalInjection.Effect;
            newInj.Target = originalInjection.Target;
            newInj.Source = originalInjection.Source;
            newInj.Priority = originalInjection.Priority;

            return (ResolvedInjection)newInj;
        }
    }
}