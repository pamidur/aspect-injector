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
            if (target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return false;

            return base.IsApplicableFor(target);
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            if (Method.ReturnType != Method.Module.TypeSystem.Object)
            {
                log.LogError(CompilationMessage.From($"Around advice {Method.FullName} should return an object. Could return null.", aspect.Host));
                return false;
            }

            return base.Validate(aspect, log);
        }
    }
}