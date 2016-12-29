using AspectInjector.Core.Defaults;
using System;

namespace AspectInjector.Core.MethodCall
{
    public class AroundAdviceInjector : InjectorBase<Advice>
    {
        public AroundAdviceInjector()
        {
            Priority = 90;
        }

        protected override bool CanApply(Advice advice)
        {
            throw new NotImplementedException();
        }
    }
}