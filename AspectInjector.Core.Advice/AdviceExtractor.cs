using AspectInjector.Core.Defaults;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.Core.MethodCall
{
    public class AdviceExtractor : InjectionReaderBase<Advice>
    {
        protected override IEnumerable<Advice> ReadInjections(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}