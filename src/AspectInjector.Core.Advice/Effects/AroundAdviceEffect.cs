using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AroundAdviceEffect : AdviceEffectBase
    {
        public override Broker.Advice.Type Type => Broker.Advice.Type.Around;

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            //check args

            if (target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            if (Method.IsStatic)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} cannot be static.", aspect.Host));
                return false;
            }

            if (!Method.IsPublic)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} should be public.", aspect.Host));
                return false;
            }

            return true;
        }
    }
}