using AspectInjector.Core.Defaults;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.Advices.MethodCall
{
    public class MethodCallAdviceExtractor : AdviceExtractorBase<MethodCallAdvice>
    {
        public override IEnumerable<MethodCallAdvice> ExtractAdvices(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}