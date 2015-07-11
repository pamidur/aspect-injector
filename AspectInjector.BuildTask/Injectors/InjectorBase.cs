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
                        yield return Markers.InstanceSelfMarker;
                        break;

                    case AdviceArgumentSource.TargetArguments:
                        yield return context.TargetMethodContext.TargetMethod.Parameters.ToArray();
                        break;

                    case AdviceArgumentSource.TargetName:
                        yield return context.TargetName;
                        break;

                    case AdviceArgumentSource.AbortFlag:
                        yield return abortFlagVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.TargetException:
                        yield return exceptionVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.TargetReturnValue:
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