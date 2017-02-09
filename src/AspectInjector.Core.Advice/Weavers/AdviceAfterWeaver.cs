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
    internal class AdviceAfterWeaver : AdviceWeaverBase<AfterAdviceEffect>
    {
        public AdviceAfterWeaver(ILogger logger) : base(logger)
        {
            Priority = 90;
        }

        protected override void WeaveMethod(MethodDefinition target, ResolvedInjection injection)
        {
            target.GetEditor().OnExit(
                e => e
                .LoadAspect(injection.Source)
                .Call(injection.Effect.Method, c => LoadAdviceArgs(c, target, injection))
            );
        }
    }
}