using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
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
            ILProcessor processor = context.AspectContext.TargetMethodContext.Processor;
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
                Instruction injectionPoint;

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

                context.AspectContext.TargetMethodContext.LoadFieldOntoStack(injectionPoint, aspectInstanceField);
                context.AspectContext.TargetMethodContext.InjectMethodCall(injectionPoint,
                    context.AdviceMethod,
                    argumentValue);
            }
        }

        private void InjectMethodCallWithResultReplacement(AdviceInjectionContext context,
            Instruction injectionPoint,
            Instruction returnPoint,
            FieldReference sourceMember)
        {
            MethodDefinition targetMethod = context.AspectContext.TargetMethodContext.TargetMethod;
            MethodDefinition method = context.AdviceMethod;
            ILProcessor processor = context.AspectContext.TargetMethodContext.Processor;

            VariableDefinition abortFlagVariable = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == AbortFlagVariableName);
            if (abortFlagVariable == null)
            {
                abortFlagVariable = processor.CreateLocalVariable(
                    injectionPoint,
                    context.AspectContext.TargetMethodContext.TargetMethod.Module.TypeSystem.Boolean,
                    false,
                    AbortFlagVariableName);
            }
            else
            {
                processor.SetLocalVariable(abortFlagVariable, injectionPoint, false);
            }

            context.AspectContext.TargetMethodContext.LoadFieldOntoStack(injectionPoint, sourceMember);
            context.AspectContext.TargetMethodContext.InjectMethodCall(
                injectionPoint,
                method,
                ResolveArgumentsValues(context.AspectContext, context.AdviceArgumentsSources, context.InjectionPoint, abortFlagVariable: abortFlagVariable, returnObjectVariable: context.AspectContext.TargetMethodContext.MethodResultVariable).ToArray());

            if (!method.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Stloc, context.AspectContext.TargetMethodContext.MethodResultVariable.Index));
            }

            processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, abortFlagVariable.Index));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I4_1));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ceq));

            Instruction continuePoint = processor.Create(OpCodes.Nop);
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Brfalse, continuePoint)); //todo::optimize
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Br, returnPoint)); //todo::optimize
            processor.InsertBefore(injectionPoint, continuePoint);
        }
    }
}