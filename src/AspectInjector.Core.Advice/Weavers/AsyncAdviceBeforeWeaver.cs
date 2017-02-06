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
    public class AsyncAdviceBeforeWeaver : EffectWeaverBase<MethodDefinition, BeforeAdviceEffect>
    {
        public AsyncAdviceBeforeWeaver(ILogger logger) : base(logger)
        {
        }

        protected override void Weave(MethodDefinition target, BeforeAdviceEffect effect, Injection injection)
        {
            throw new NotImplementedException();
        }
    }
}