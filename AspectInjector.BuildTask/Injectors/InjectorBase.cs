using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Injectors
{
    internal abstract class InjectorBase
    {
        protected IEnumerable<object> ResolveArgumentsValues(
            AspectContext context,
            List<AdviceArgumentSource> sources,
            InjectionPoints injectionPointFired,
            VariableDefinition abortFlagVariable = null,
            VariableDefinition exceptionVariable = null,
            VariableDefinition returnObjectVariable = null)
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

                    case AdviceArgumentSource.ReturnType:
                        yield return context.TargetMethodContext.TargetMethod.ReturnType;
                        break;

                    case AdviceArgumentSource.ReturnValue:
                        yield return returnObjectVariable ?? Markers.DefaultMarker;
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