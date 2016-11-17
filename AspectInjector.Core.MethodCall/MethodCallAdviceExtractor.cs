using AspectInjector.Core.Dafaults;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.MethodCall
{
    public class MethodCallAdviceExtractor : DefaultAdviceExtractorBase<MethodCallAdvice>
    {
        protected override IEnumerable<MethodCallAdvice> ExtractAdvices(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}