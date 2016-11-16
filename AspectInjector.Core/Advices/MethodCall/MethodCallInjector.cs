using System;

namespace AspectInjector.Core.Advices.MethodCall
{
    public class MethodCallInjector : InjectorBase<MethodCallAdvice>
    {
        protected override bool CanApply(MethodCallAdvice advice)
        {
            throw new NotImplementedException();
        }
    }
}