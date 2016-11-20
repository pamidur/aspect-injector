using AspectInjector.Core.Defaults;
using System;

namespace AspectInjector.Core.MethodCall
{
    public class AdviceInjector : InjectorBase<Advice>
    {
        protected override bool CanApply(Advice advice)
        {
            throw new NotImplementedException();
        }
    }
}