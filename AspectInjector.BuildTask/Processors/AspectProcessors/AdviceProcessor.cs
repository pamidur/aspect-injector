using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System;
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

        public void Process(AspectInjectionContext context)
        {
            var adviceContexts = GetAdviceMethods(context.AspectType).SelectMany(m => ProcessAdvice(m, context)).Where(ac => ac != null).ToList();

            adviceContexts.ForEach(ValidateContext);

            foreach (var adviceContext in adviceContexts)
                _injector.Inject(adviceContext);
        }

        private static bool CheckTarget(TargetMethodContext targetMethodContext, InjectionTargets target)
        {
            var targetMethod = targetMethodContext.TargetMethod;

            if (targetMethod.IsAbstract || targetMethod.IsStatic)
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

            return target == InjectionTargets.Method;
        }

        private static IEnumerable<MethodDefinition> GetAdviceMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.HasAttributeOfType<AdviceAttribute>());
        }

        private static IEnumerable<AdviceInjectionContext> ProcessAdvice(MethodDefinition adviceMethod,
            AspectInjectionContext parentContext)
        {
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
                        context.AdviceArgumentsSources = ProcessingUtils.GetAdviceArgumentsSources(adviceMethod).ToList();

                        if (target == InjectionTargets.Constructor)
                        {
                            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                                throw new CompilationException("Advice of InjectionTargets.Constructor can be System.Void only", adviceMethod);

                            if (context.IsAbortable)
                                throw new CompilationException("Constructors methods don't support AdviceArgumentSource.AbortFlag", adviceMethod);
                        }

                        if (point == InjectionPoints.Before)
                        {
                            if (context.IsAbortable && !context.AdviceMethod.ReturnType.IsTypeOf(adviceMethod.ReturnType))
                                throw new CompilationException("Return types of advice (" + adviceMethod.FullName + ") and target (" + context.AspectContext.TargetMethodContext.TargetMethod.FullName + ") should be the same", context.AspectContext.TargetMethodContext.TargetMethod);

                            if (!context.IsAbortable && !adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                                throw new CompilationException("Advice of InjectionPoints.Before without argument of AdviceArgumentSource.AbortFlag can be System.Void only", adviceMethod);

                            context.InjectionPoint = InjectionPoints.Before;
                        }

                        if (point == InjectionPoints.After)
                        {
                            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                                throw new CompilationException("Advice of InjectionPoints.After can be System.Void only", adviceMethod);

                            if (context.IsAbortable)
                                throw new CompilationException("Method should inject into only InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag", adviceMethod);

                            context.InjectionPoint = InjectionPoints.After;
                        }

                        if (point == InjectionPoints.Exception)
                        {
                            if (context.IsAbortable)
                                throw new CompilationException("Method should inject into only InjectionPoints.Before in order to use AdviceArgumentSource.AbortFlag", adviceMethod);

                            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
                                throw new CompilationException("Advice of InjectionPoints.After can be System.Void only", adviceMethod);

                            context.InjectionPoint = InjectionPoints.Exception;
                        }

                        yield return context;
                    }
                }
            }
        }

        private void ValidateContext(AdviceInjectionContext context)
        {
            if (context.IsAbortable)
            {
                if (_abortableInjectionHistory.Contains(context.AspectContext.TargetMethodContext))
                    throw new CompilationException("Method may have only one advice with argument of AdviceArgumentSource.AbortFlag applied to it", context.AspectContext.TargetMethodContext.TargetMethod);

                _abortableInjectionHistory.Add(context.AspectContext.TargetMethodContext);
            }
        }
    }
}