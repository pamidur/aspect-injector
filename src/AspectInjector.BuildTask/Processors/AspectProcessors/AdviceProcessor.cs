using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Validation;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.BuildTask.Processors.AspectProcessors
{
    internal class AdviceProcessor : IAspectProcessor
    {
        private readonly IAspectInjector<AdviceInjectionContext> _injector;

        public AdviceProcessor(IAspectInjector<AdviceInjectionContext> injector)
        {
            _injector = injector;
        }

        public bool CanProcess(TypeDefinition aspectType)
        {
            return aspectType.Methods.Any(m => m.CustomAttributes.HasAttributeOfType<AdviceAttribute>());
        }

        public void Process(AspectContext context)
        {
            var adviceContexts = GetAdviceMethods(context.AdviceClassType).SelectMany(m => ProcessAdvice(m, context)).Where(ac => ac != null).ToList();

            foreach (var adviceContext in adviceContexts)
                _injector.Inject(adviceContext);
        }

        private static bool CheckTarget(TargetMethodContext targetMethodContext, InjectionTargets target)
        {
            var targetMethod = targetMethodContext.TargetMethod;

            if (targetMethod.IsAbstract || 
                targetMethod.IsPInvokeImpl)
            {
                return false;
            }

            if (targetMethod.IsConstructor)
            {
                return target == InjectionTargets.Constructor;
            }

            if (targetMethod.IsGetter)
            {
                return target == InjectionTargets.Getter;
            }

            if (targetMethod.IsSetter)
            {
                return target == InjectionTargets.Setter;
            }

            if (targetMethod.IsAddOn)
            {
                return target == InjectionTargets.EventAdd;
            }

            if (targetMethod.IsRemoveOn)
            {
                return target == InjectionTargets.EventRemove;
            }

            if (!targetMethod.CustomAttributes.HasAttributeOfType<CompilerGeneratedAttribute>())
            {
                return target == InjectionTargets.Method;
            }

            return false;
        }

        private static IEnumerable<MethodDefinition> GetAdviceMethods(TypeDefinition adviceClassType)
        {
            Validator.ValidateAdviceClassType(adviceClassType);

            return adviceClassType.Methods.Where(m => m.CustomAttributes.HasAttributeOfType<AdviceAttribute>());
        }

        private static IEnumerable<AdviceArgumentSource> GetAdviceArgumentsSources(MethodDefinition adviceMethod)
        {
            foreach (var parameter in adviceMethod.Parameters)
            {
                Validator.ValidateAdviceMethodParameter(parameter, adviceMethod);

                var argumentAttribute = parameter.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
                var source = (AdviceArgumentSource)argumentAttribute.ConstructorArguments[0].Value;
                yield return source;
            }
        }

        private static IEnumerable<AdviceInjectionContext> ProcessAdvice(MethodDefinition adviceMethod,
            AspectContext parentContext)
        {
            Validator.ValidateAdviceMethod(adviceMethod);

            var adviceAttribute = adviceMethod.CustomAttributes.GetAttributeOfType<AdviceAttribute>();

            var points = (InjectionPoints)adviceAttribute.ConstructorArguments[0].Value;
            var targets = (InjectionTargets)adviceAttribute.ConstructorArguments[1].Value;

            foreach (InjectionPoints point in Enum.GetValues(typeof(InjectionPoints)).Cast<InjectionPoints>().Where(p => (points & p) != 0))
            {
                foreach (InjectionTargets target in Enum.GetValues(typeof(InjectionTargets)).Cast<InjectionTargets>().Where(t => (targets & t) != 0))
                {
                    if (CheckTarget(parentContext.TargetMethodContext, target))
                    {
                        var context = new AdviceInjectionContext() { AspectContext = parentContext };

                        context.AdviceMethod = adviceMethod;
                        context.AdviceArgumentsSources = GetAdviceArgumentsSources(adviceMethod).ToList();
                        context.InjectionPoint = point;

                        Validator.ValidateAdviceInjectionContext(context, target);

                        yield return context;
                    }
                }
            }
        }
    }
}