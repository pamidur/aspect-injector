using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advice.Weavers
{
    public class AdviceAfterAsyncWeaver : AdviceWeaverBase<AfterAdviceEffect>
    {
        public AdviceAfterAsyncWeaver(ILogger logger) : base(logger)
        {
        }

        protected override void WeaveMethod(MethodDefinition target, AfterAdviceEffect effect, Injection injection)
        {
            throw new NotImplementedException();
        }

        protected override bool CanWeave(Injection injection)
        {
            return base.CanWeave(injection) && injection.Target is MethodDefinition && ((MethodDefinition)injection.Target).IsAsync();
        }
    }
}