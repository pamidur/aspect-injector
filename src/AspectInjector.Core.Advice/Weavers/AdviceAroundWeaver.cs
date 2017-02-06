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
    public class AdviceAroundWeaver : AdviceWeaverBase<AroundAdviceEffect>
    {
        public AdviceAroundWeaver(ILogger logger) : base(logger)
        {
        }

        protected override void WeaveMethod(MethodDefinition target, AroundAdviceEffect effect, Injection injection)
        {
            throw new NotImplementedException();
        }
    }
}