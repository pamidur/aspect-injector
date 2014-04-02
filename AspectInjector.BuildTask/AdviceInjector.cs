using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class AdviceInjector : InjectorBase, IModuleProcessor
    {
        private static readonly string _abortFlagVariableName = "__a$_do_abort_method";
        private static readonly string _abortResultVariableName = "__a$_abort_method_result";

        public virtual void ProcessModule(ModuleDefinition module)
        {
            var contexts = module.GetInjectionContexts();

            foreach (var context in contexts)
            {
                InjectAdvice(context);
            }
        }

        private void InjectAdvice(InjectionContext context)
        {
            ILProcessor processor = context.TargetMethodContext.Processor;
            FieldReference aspectInstanceField = GetOrCreateAspectReference(context);

            if (context.IsAbortable)
            {
                InjectMethodCallWithResultReplacement(
                    context,
                    context.TargetMethodContext.OriginalEntryPoint,
                    context.TargetMethodContext.OriginalReturnPoint,
                    aspectInstanceField);
            }
            else
            {
                InjectMethodCall(
                    processor,
                    context.InjectionPoint == InjectionPoints.Before ?
                        context.TargetMethodContext.OriginalEntryPoint :
                        context.TargetMethodContext.ReturnPoint,
                    aspectInstanceField,
                    context.AdviceMethod,
                    GetAdviceArgumentsValues(context, null).ToArray());
            }
        }

        private void InjectMethodCallWithResultReplacement(InjectionContext context,
            Instruction injectionPoint,
            Instruction returnPoint,
            FieldReference sourceMember)
        {
            MethodDefinition targetMethod = context.TargetMethodContext.TargetMethod;
            MethodDefinition method = context.AdviceMethod;
            ILProcessor processor = context.TargetMethodContext.Processor;

            VariableDefinition abortFlagVariable = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == _abortFlagVariableName);
            if (abortFlagVariable == null)
            {
                abortFlagVariable = processor.CreateLocalVariable(
                    injectionPoint,
                    context.TargetMethodContext.TargetMethod.Module.TypeSystem.Boolean,
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
                GetAdviceArgumentsValues(context, abortFlagVariable).ToArray());

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

        private IEnumerable<object> GetAdviceArgumentsValues(InjectionContext context, VariableDefinition abortFlagVariable)
        {
            foreach (var argumentSource in context.AdviceArgumentsSources)
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
                        yield return abortFlagVariable;
                        break;

                    case AdviceArgumentSource.CustomData:
                        yield return context.AspectCustomData;
                        break;
                }
            }
        }
    }
}