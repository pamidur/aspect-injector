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
    public class AdviceAfterWeaver : AdviceWeaverBase<AfterAdviceEffect>
    {
        public AdviceAfterWeaver(ILogger logger) : base(logger)
        {
        }

        protected override void WeaveMethod(MethodDefinition target, AfterAdviceEffect effect, Injection injection)
        {
            target.GetEditor().OnExit(
                e => e
                .LoadAspect(injection.Source)
                .Call(effect.Method, c => LoadAdviceArgs(c, target, effect, injection))
            );
        }
    }
}