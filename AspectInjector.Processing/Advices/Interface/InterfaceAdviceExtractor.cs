using AspectInjector.Core.Defaults;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Advices.Interface
{
    public class InterfaceAdviceExtractor : AdviceExtractorBase<InterfaceAdvice>
    {
        public override IEnumerable<InterfaceAdvice> ExtractAdvices(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}