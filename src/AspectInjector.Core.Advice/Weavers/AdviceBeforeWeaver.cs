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
using AspectInjector.Core.Fluent;

namespace AspectInjector.Core.Advice.Weavers
{
    internal class AdviceBeforeWeaver : AdviceWeaverBase<BeforeAdviceEffect>
    {
        public AdviceBeforeWeaver(ILogger logger) : base(logger)
        {
        }

        protected override void WeaveMethod(MethodDefinition target, BeforeAdviceEffect effect, Injection injection)
        {
            target.GetEditor().OnEntry(
                e => e
                .LoadAspect(injection.Source)
                .Call(effect.Method, c => LoadAdviceArgs(c, target, effect, injection))
            );
        }
    }
}