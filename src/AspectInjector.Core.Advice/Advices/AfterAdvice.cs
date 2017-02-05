using Mono.Cecil;

namespace AspectInjector.Core.Advice.Advices
{
    public class AfterAdvice : AdviceBase
    {
        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            // check args

            return base.IsApplicableFor(target);
        }
    }
}