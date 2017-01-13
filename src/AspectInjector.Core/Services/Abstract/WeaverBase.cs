using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Services.Abstract
{
    public abstract class WeaverBase<TTarget, TEffect> : IWeaver
        where TTarget : ICustomAttributeProvider
        where TEffect : Effect
    {
        protected ILogger Log { get; private set; }

        protected Context Context { get; private set; }

        public byte Priority { get; protected set; }

        protected abstract void Weave(TTarget target, TEffect effect, Injection injection);

        public void Init(Context context)
        {
            Log = context.Services.Log;
            Context = context;
        }

        public void Weave(Injection injection)
        {
            Weave((TTarget)injection.Target, (TEffect)injection.Effect, injection);
        }

        public bool CanWeave(Injection injection)
        {
            return injection.Target is TTarget && injection.Effect is TEffect;
        }
    }
}