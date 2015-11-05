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

            PointCut injectionPoint;

            switch (context.InjectionPoint)
            {
                case InjectionPoints.Before:
                    injectionPoint = context.AspectContext.TargetMethodContext.EntryPoint;
                    break;

                case InjectionPoints.After:
                    injectionPoint = context.AspectContext.TargetMethodContext.ReturnPoint;
                    break;

                case InjectionPoints.Around:
                    injectionPoint = context.AspectContext.TargetMethodContext.CreateNewAroundPoint();
                    break;

                default: throw new NotSupportedException(context.InjectionPoint.ToString() + " is not supported (yet?)");
            }

            var argumentValue = ResolveArgumentsValues(
                        context.AspectContext,
                        context.AdviceArgumentsSources,
                        context.InjectionPoint,
                        returnObjectVariable: context.AspectContext.TargetMethodContext.MethodResultVariable)
                        .ToArray();

            if (!aspectInstanceField.Resolve().IsStatic) injectionPoint.LoadSelfOntoStack();
            injectionPoint.LoadFieldOntoStack(aspectInstanceField);
            injectionPoint.InjectMethodCall(context.AdviceMethod, argumentValue);
        }
    }
}