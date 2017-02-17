using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Advice.Weavers.Processes;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advice.Weavers
{
    public class AdviceAroundWeaver : AdviceInlineWeaver
    {
        public override byte Priority => 20;

        public AdviceAroundWeaver(ILogger logger) : base(logger)
        {
        }

        public override bool CanWeave(Injection injection)
        {
            return injection.Effect is AroundAdviceEffect &&
                (injection.Target is EventDefinition || injection.Target is PropertyDefinition || injection.Target is MethodDefinition);
        }

        protected override void WeaveMethod(MethodDefinition method, Injection injection)
        {
            //if (injection.Effect is AroundAdviceEffect)
            //{
            //    var process = new AdviceAroundProcess(_log, injection.Source, method, (AroundAdviceEffect)injection.Effect);
            //    process.Execute();
            //}
            //else
            //{
            //    throw new Exception("Unknown advice type.");
            //}
        }
    }
}