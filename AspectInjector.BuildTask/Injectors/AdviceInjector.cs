using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Injectors
{
    internal class AdviceInjector : InjectorBase, IAspectInjector<AdviceInjectionContext>
    {
        private static readonly string _abortFlagVariableName = "__a$_do_abort_method";
        private static readonly string _abortResultVariableName = "__a$_abort_method_result";

        public void Inject(AdviceInjectionContext context)
        {
            ILProcessor processor = context.AspectContext.TargetMethodContext.Processor;
            FieldReference aspectInstanceField = GetOrCreateAspectReference(context.AspectContext);

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
                InjectMethodCall(
                    processor,
                    context.InjectionPoint == InjectionPoints.Before ?
                        context.AspectContext.TargetMethodContext.EntryPoint :
                        context.AspectContext.TargetMethodContext.ExitPoint,
                    aspectInstanceField,
                    context.AdviceMethod,
                    ResolveArgumentsValues(context.AspectContext, context.AdviceArgumentsSources, null).ToArray());
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

            VariableDefinition abortFlagVariable = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == _abortFlagVariableName);
            if (abortFlagVariable == null)
            {
                abortFlagVariable = processor.CreateLocalVariable(
                    injectionPoint,
                    context.AspectContext.TargetMethodContext.TargetMethod.Module.TypeSystem.Boolean,
                    false,
                    _abortFlagVariableName);
            }
            else
            {
                processor.SetLocalVariable(abortFlagVariable, injectionPoint, false);
            }

            VariableDefinition resultVariable = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == _abortResultVariableName);
            if (resultVariable == null && !method.ReturnType.IsTypeOf(typeof(void)))
            {
                if (method.ReturnType.IsValueType)
                {
                    resultVariable = processor.CreateLocalVariable(
                        injectionPoint,
                        targetMethod.Module.Import(method.ReturnType),
                        0,
                        _abortResultVariableName);
                }
                else
                {
                    resultVariable = processor.CreateLocalVariable<object>(
                        injectionPoint,
                        targetMethod.Module.Import(method.ReturnType),
                        null,
                        _abortResultVariableName);
                }
            }

            InjectMethodCall(processor,
                injectionPoint,
                sourceMember,
                method,
                ResolveArgumentsValues(context.AspectContext, context.AdviceArgumentsSources, abortFlagVariable).ToArray());

            if (!method.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Stloc, resultVariable.Index));
            }

            processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, abortFlagVariable.Index));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I4_1));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ceq));

            Instruction continuePoint = processor.Create(OpCodes.Nop);
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Brfalse_S, continuePoint));

            if (!method.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, resultVariable.Index));
            }

            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Br_S, returnPoint));
            processor.InsertBefore(injectionPoint, continuePoint);
        }
    }
}