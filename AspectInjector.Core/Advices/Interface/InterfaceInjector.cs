using AspectInjector.Core.Models;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advices.Interface
{
    public class InterfaceInjector : InjectorBase<InterfaceAdvice>
    {
        protected override void Apply(Aspect<AssemblyDefinition> aspect, InterfaceAdvice advice)
        {
            base.Apply(aspect, advice);
        }

        protected override bool CanApply(InterfaceAdvice advice)
        {
            throw new NotImplementedException();
        }
    }
}