using Mono.Cecil;

namespace AspectInjector.Core.Advice.Advices
{
    public class BeforeAdvice : AdviceBase
    {
        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            //check args

            return base.IsApplicableFor(target);
        }
    }
}