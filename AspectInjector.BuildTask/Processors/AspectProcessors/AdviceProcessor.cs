using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Processors.AspectProcessors
{
    internal class AdviceProcessor : IAspectProcessor
    {
        private readonly IAspectInjector<AdviceInjectionContext> _injector;

        private List<TargetMethodContext> _abortableInjectionHistory = new List<TargetMethodContext>();

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
            var adviceContexts = GetAdviceMethods(context.AspectType).Select(m => ProcessAdvice(m, context)).Where(ac => ac != null).ToList();

            adviceContexts.ForEach(ValidateContext);

            foreach (var adviceContext in adviceContexts)
                _injector.Inject(adviceContext);
        }

        private void ValidateContext(AdviceInjectionContext context)
        {
            if (context.IsAbortable && _abortableInjectionHistory.Contains(context.AspectContext.TargetMethodContext))
                throw new CompilationException("Method may have only one advice with argument of AdviceArgumentSource.AbortFlag applied to it", context.AspectContext.TargetMethodContext.TargetMethod);

            _abortableInjectionHistory.Add(context.AspectContext.TargetMethodContext);
        }

        private static IEnumerable<MethodDefinition> GetAdviceMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.HasAttributeOfType<AdviceAttribute>());
        }

        private static AdviceInjectionContext ProcessAdvice(MethodDefinition adviceMethod,
            AspectContext parentContext)
        {
            var adviceAttribute = adviceMethod.CustomAttributes.GetAttributeOfType<AdviceAttribute>();

            var points = (InjectionPoints)adviceAttribute.ConstructorArguments[0].Value;
            var targets = (InjectionTargets)adviceAttribute.ConstructorArguments[1].Value;

            if (CheckTarget(parentContext.TargetMethodContext, targets))
            {
                var context = new AdviceInjectionContext() { AspectContext = parentContext };

                context.AdviceMethod = adviceMethod;
                context.AdviceArgumentsSources = ProcessingUtils.GetAdviceArgumentsSources(adviceMethod).ToList();

                if ((targets & InjectionTargets.Constructor) != 0)
                {
                    if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                        throw new CompilationException("Advice of InjectionTargets.Constructor can be System.Void only", adviceMethod);

                    if (context.IsAbortable)
                        throw new CompilationException("Constructors methods don't support AdviceArgumentSource.AbortFlag", adviceMethod);

                    if (context.AdviceArgumentsSources.Any(s => s == AdviceArgumentSource.ReturningValue))
                        throw new CompilationException("Constructors methods don't support AdviceArgumentSource.ReturningValue", adviceMethod);
                }

                if ((points & InjectionPoints.Before) != 0)
                {
                    if (context.IsAbortable && !context.AdviceMethod.ReturnType.IsTypeOf(adviceMethod.ReturnType))
                        throw new CompilationException("Return types of advice (" + adviceMethod.FullName + ") and target (" + context.AspectContext.TargetMethodContext.TargetMethod.FullName + ") should be the same", context.AspectContext.TargetMethodContext.TargetMethod);

                    if (!context.IsAbortable && !adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                        throw new CompilationException("Advice of InjectionPoints.Before without argument of AdviceArgumentSource.AbortFlag can be System.Void only", adviceMethod);

                    if (context.AdviceArgumentsSources.Any(s => s == AdviceArgumentSource.ReturningValue))
                        throw new CompilationException("Before methods don't support AdviceArgumentSource.ReturningValue", adviceMethod);

                    context.InjectionPoint = InjectionPoints.Before;
                }

                if ((points & InjectionPoints.After) != 0)
                {
                    if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                        throw new CompilationException("Advice of InjectionPoints.After can be System.Void only", adviceMethod);

                    if (context.IsAbortable)
                        throw new CompilationException("Method should have a return value and inject into InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag", adviceMethod);

                    context.InjectionPoint = InjectionPoints.After;
                }

                if ((points & InjectionPoints.Exception) != 0)
                {
                    throw new CompilationException("InjectionPoints.Exception not supported yet", adviceMethod);
                }

                return context;
            }
            return null;
        }


        private static bool CheckTarget(TargetMethodContext targetMethodContext, InjectionTargets targets)
        {
            var targetMethod = targetMethodContext.TargetMethod;

            if (targetMethod.IsAbstract || targetMethod.IsStatic)
            {
                return false;
            }

            if (targetMethod.IsConstructor)
            {
                return (targets & InjectionTargets.Constructor) != 0;
            }

            if (targetMethod.IsGetter)
            {
                return (targets & InjectionTargets.Getter) != 0;
            }

            if (targetMethod.IsSetter)
            {
                return (targets & InjectionTargets.Setter) != 0;
            }

            if (targetMethod.IsAddOn)
            {
                return (targets & InjectionTargets.EventAdd) != 0;
            }

            if (targetMethod.IsRemoveOn)
            {
                return (targets & InjectionTargets.EventRemove) != 0;
            }

            return (targets & InjectionTargets.Method) != 0;
        }
    }
}