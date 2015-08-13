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
        private static readonly string AbortFlagVariableName = "__a$_doAbortMethod";

        public void Inject(AdviceInjectionContext context)
        {
            FieldReference aspectInstanceField = context.AspectContext.TargetTypeContext.GetOrCreateAspectReference(context.AspectContext);

            if (context.IsAbortable)
            {
                InjectMethodCallWithResultReplacement(
                    context,
                    context.AspectContext.TargetMethodContext.OriginalEntryPoint,
                    context.AspectContext.TargetMethodContext.ReturnPoint,
                    aspectInstanceField);
            }
            else
            {
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
                        injectionPoint = context.AspectContext.TargetMethodContext.ExitPoint;
                        argumentValue = ResolveArgumentsValues(
                            context.AspectContext,
                            context.AdviceArgumentsSources,
                            context.InjectionPoint,
                            returnObjectVariable: context.AspectContext.TargetMethodContext.MethodResultVariable)
                            .ToArray();
                        break;

                    case InjectionPoints.Exception:
                        injectionPoint = context.AspectContext.TargetMethodContext.ExceptionPoint;
                        argumentValue = ResolveArgumentsValues(
                            context.AspectContext,
                            context.AdviceArgumentsSources,
                            context.InjectionPoint,
                            exceptionVariable: context.AspectContext.TargetMethodContext.ExceptionVariable)
                            .ToArray();
                        break;

                    default: throw new NotSupportedException(context.InjectionPoint.ToString() + " is not supported (yet?)");
                }

                injectionPoint.LoadFieldOntoStack(aspectInstanceField);
                injectionPoint.InjectMethodCall(context.AdviceMethod, argumentValue);
            }
        }

        private void InjectMethodCallWithResultReplacement(AdviceInjectionContext context,
            PointCut injectionPoint,
            PointCut returnPoint,
            FieldReference sourceMember)
        {
            MethodDefinition targetMethod = context.AspectContext.TargetMethodContext.TargetMethod;
            MethodDefinition method = context.AdviceMethod;

            VariableDefinition abortFlagVariable = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == AbortFlagVariableName);
            if (abortFlagVariable == null)
            {
                abortFlagVariable = injectionPoint.CreateVariable(
                    context.AspectContext.TargetMethodContext.TargetMethod.Module.TypeSystem.Boolean,
                    false,
                    AbortFlagVariableName);
            }
            else
            {
                injectionPoint.SetVariable(abortFlagVariable, false);
            }

            injectionPoint.LoadFieldOntoStack(sourceMember);
            injectionPoint.InjectMethodCall(
                method,
                ResolveArgumentsValues(context.AspectContext, context.AdviceArgumentsSources, context.InjectionPoint, abortFlagVariable: abortFlagVariable, returnObjectVariable: context.AspectContext.TargetMethodContext.MethodResultVariable).ToArray());

            if (!method.ReturnType.IsTypeOf(typeof(void)))
            {
                injectionPoint.InsertBefore(injectionPoint.Processor.CreateOptimized(OpCodes.Stloc, context.AspectContext.TargetMethodContext.MethodResultVariable.Index));
            }

            injectionPoint.LoadVariableOntoStack(abortFlagVariable);
            injectionPoint.TestValueOnStack(true, doIfTrue: pc => pc.Goto(returnPoint));
        }
    }
}