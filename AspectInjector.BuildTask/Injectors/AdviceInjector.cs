using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Injectors
{
    internal class AdviceInjector : IAspectInjector<AdviceInjectionContext>
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
                        context.AdviceArgumentsSources)
                        .ToArray();

            if (!aspectInstanceField.Resolve().IsStatic) injectionPoint.LoadSelfOntoStack();
            injectionPoint.LoadFieldOntoStack(aspectInstanceField);
            injectionPoint.InjectMethodCall(context.AdviceMethod, argumentValue);
        }

        protected IEnumerable<object> ResolveArgumentsValues(
           AspectContext context,
           List<AdviceArgumentSource> sources)
        {
            foreach (var argumentSource in sources)
            {
                switch (argumentSource)
                {
                    case AdviceArgumentSource.Instance:
                        yield return context.TargetMethodContext.TargetMethod.IsStatic ? Markers.DefaultMarker : Markers.InstanceSelfMarker;
                        break;

                    case AdviceArgumentSource.Type:
                        yield return context.TargetTypeContext.TypeDefinition;
                        break;

                    case AdviceArgumentSource.Method:
                        yield return context.TargetMethodContext.TargetMethod;
                        break;

                    case AdviceArgumentSource.Arguments:
                        yield return context.TargetMethodContext.TargetMethod.Parameters.ToArray();
                        break;

                    case AdviceArgumentSource.Name:
                        yield return context.TargetName;
                        break;

                    case AdviceArgumentSource.TargetReturnType:
                        yield return context.TargetMethodContext.TargetMethod.ReturnType;
                        break;

                    case AdviceArgumentSource.ReturnValue:
                        yield return context.TargetMethodContext.MethodResultVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.RoutableData:
                        yield return context.AspectRoutableData ?? Markers.DefaultMarker;
                        break;

                    default:
                        throw new NotSupportedException(argumentSource.ToString() + " is not supported (yet?)");
                }
            }
        }
    }
}