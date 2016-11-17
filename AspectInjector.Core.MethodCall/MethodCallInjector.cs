using AspectInjector.Core.Defaults;
using System;

namespace AspectInjector.Core.MethodCall
{
    public class MethodCallInjector : DefaultInjectorBase<MethodCallAdvice>
    {
        protected override bool CanApply(MethodCallAdvice advice)
        {
            throw new NotImplementedException();
        }
    }
}