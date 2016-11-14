using AspectInjector.Defaults;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Advices.MethodCall
{
    public class MethodCallAdviceExtractor : DefaultAdviceExtractorBase<MethodCallAdvice>
    {
        public override IEnumerable<MethodCallAdvice> ExtractAdvices(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}