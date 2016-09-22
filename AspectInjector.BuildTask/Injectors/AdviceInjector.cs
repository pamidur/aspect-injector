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
                    injectionPoint = context.AspectContext.TargetMethodContext.Value.EntryPoint;
                    break;

                case InjectionPoints.After:
                    injectionPoint = context.AspectContext.TargetMethodContext.Value.ReturnPoint;
                    break;

                case InjectionPoints.Around:
                    injectionPoint = context.AspectContext.TargetMethodContext.Value.CreateNewAroundPoint();
                    break;

                default: throw new NotSupportedException(context.InjectionPoint + " is not supported (yet?)");
            }

            var argumentValue = ResolveArgumentsValues(
                        context.AspectContext,
                        context.AdviceArgumentsSources)
                        .ToArray();

            if (!aspectInstanceField.Resolve().IsStatic) injectionPoint.LoadSelfOntoStack();
            injectionPoint.LoadField(aspectInstanceField);
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
                        yield return context.TargetMethodContext.Value.TargetMethod.IsStatic ? Markers.DefaultMarker : Markers.InstanceSelfMarker;
                        break;

                    case AdviceArgumentSource.Type:
                        yield return context.TargetTypeContext.TypeDefinition;
                        break;

                    case AdviceArgumentSource.Method:
                        yield return context.TargetMethodContext.Value.TopWrapper ?? context.TargetMethodContext.Value.TargetMethod;
                        break;

                    case AdviceArgumentSource.Arguments:
                        yield return Markers.AllArgsMarker;
                        break;

                    case AdviceArgumentSource.Name:
                        yield return context.TargetName;
                        break;

                    case AdviceArgumentSource.ReturnType:
                        yield return context.TargetMethodContext.Value.TargetMethod.ReturnType;
                        break;

                    case AdviceArgumentSource.ReturnValue:
                        yield return context.TargetMethodContext.Value.MethodResultVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.RoutableData:
                        yield return context.AspectRoutableData ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.Target:
                        yield return Markers.TargetFuncMarker;
                        break;

                    default:
                        throw new NotSupportedException(argumentSource.ToString() + " is not supported (yet?)");
                }
            }
        }
    }
}