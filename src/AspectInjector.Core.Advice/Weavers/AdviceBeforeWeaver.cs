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
    public class AdviceBeforeWeaver : AdviceWeaverBase<BeforeAdviceEffect>
    {
        public AdviceBeforeWeaver(ILogger logger) : base(logger)
        {
        }

        protected override void WeaveMethod(MethodDefinition target, BeforeAdviceEffect effect, Injection injection)
        {
            throw new NotImplementedException();
        }
    }
}