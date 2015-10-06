using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask.Injectors
{
    internal class AdviceInjector : InjectorBase, IAspectInjector<AdviceInjectionContext>
    {
        public void Inject(AdviceInjectionContext context)
        {
            FieldReference aspectInstanceField = context.AspectContext.TargetTypeContext.GetOrCreateAspectReference(context.AspectContext);

            object[] argumentValue;
            PointCut injectionPoint;

            switch (context.InjectionPoint)
            {
                case InjectionPoints.Before:
                    injectionPoint = context.AspectContext.TargetMethodContext.EntryPoint;
                    argumentValue = ResolveArgumentsValues(
                        context.AspectContext,
                        context.AdviceArgumentsSources,
                        context.InjectionPoint,
                        returnObjectVariable: context.AspectContext.TargetMethodContext.MethodResultVariable)
                        .ToArray();
                    break;

                case InjectionPoints.After:
                    injectionPoint = context.AspectContext.TargetMethodContext.ReturnPoint;
                    argumentValue = ResolveArgumentsValues(
                        context.AspectContext,
                        context.AdviceArgumentsSources,
                        context.InjectionPoint,
                        returnObjectVariable: context.AspectContext.TargetMethodContext.MethodResultVariable)
                        .ToArray();
                    break;

                default: throw new NotSupportedException(context.InjectionPoint.ToString() + " is not supported (yet?)");
            }

            injectionPoint.LoadFieldOntoStack(aspectInstanceField);
            injectionPoint.InjectMethodCall(context.AdviceMethod, argumentValue);
        }
    }
}