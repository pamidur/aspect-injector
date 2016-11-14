using AspectInjector.Defaults;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Advices.Interface
{
    public class InterfaceAdviceExtractor : DefaultAdviceExtractorBase<InterfaceAdvice>
    {
        public override IEnumerable<InterfaceAdvice> ExtractAdvices(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}