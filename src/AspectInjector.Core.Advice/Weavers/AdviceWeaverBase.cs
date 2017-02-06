using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Services;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;

namespace AspectInjector.Core.Advice.Weavers
{
    public abstract class AdviceWeaverBase<TEffect> : EffectWeaverBase<IMemberDefinition, TEffect>
        where TEffect : AdviceEffectBase
    {
        public AdviceWeaverBase(ILogger logger) : base(logger)
        {
        }

        protected override bool CanWeave(Injection injection)
        {
            return base.CanWeave(injection) &&
                (injection.Target is EventDefinition || injection.Target is PropertyDefinition || injection.Target is MethodDefinition);
        }

        protected override void Weave(IMemberDefinition target, TEffect effect, Injection injection)
        {
            if (target is EventDefinition)
            {
                WeaveEvent((EventDefinition)target, effect, injection);
                return;
            }

            if (target is PropertyDefinition)
            {
                WeaveProperty((PropertyDefinition)target, effect, injection);
                return;
            }

            if (target is MethodDefinition)
            {
                WeaveMethod((MethodDefinition)target, effect, injection);
                return;
            }

            _log.LogError(CompilationMessage.From($"Unsupported target {target.GetType().Name}", target));
        }

        protected abstract void WeaveMethod(MethodDefinition target, TEffect effect, Injection injection);

        protected virtual void WeaveProperty(PropertyDefinition target, TEffect effect, Injection injection)
        {
            if (target.SetMethod != null)
                WeaveMethod(target.SetMethod, effect, injection);

            if (target.GetMethod != null)
                WeaveMethod(target.GetMethod, effect, injection);
        }

        protected virtual void WeaveEvent(EventDefinition target, TEffect effect, Injection injection)
        {
            if (target.AddMethod != null)
                WeaveMethod(target.AddMethod, effect, injection);

            if (target.RemoveMethod != null)
                WeaveMethod(target.RemoveMethod, effect, injection);
        }
    }
}