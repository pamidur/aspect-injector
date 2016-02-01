using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.BuildTask.Contexts
{
    internal static class AsyncVoidRewriter
    {
        public static void Rewrite(PointCut asyncVoidPc, MethodDefinition asyncVoidMethod, VariableDefinition taskVar)
        {
            asyncVoidMethod.Body.SimplifyMacros();

            var asyncVoidMBType = asyncVoidMethod.Module.Import(typeof(AsyncVoidMethodBuilder));
            var asyncTaskMBType = asyncVoidMethod.Module.Import(typeof(AsyncTaskMethodBuilder));

            asyncVoidMethod.Body.Variables.First(v => v.VariableType.IsTypeOf(asyncVoidMBType)).VariableType = asyncTaskMBType;

            Instruction loadStateMachineInst = null;
            FieldReference builderField = null;

            foreach (var inst in asyncVoidMethod.Body.Instructions)
            {
                var method = inst.Operand as MethodReference;

                if (method != null && method.DeclaringType.IsTypeOf(asyncVoidMBType))
                {
                    var newMethod = asyncVoidMethod.Module.Import(asyncTaskMBType.Resolve().Methods.First(m => m.Name == method.Name));

                    if (method.IsGenericInstance)
                    {
                        newMethod = newMethod.MakeGeneric(newMethod.DeclaringType, ((IGenericInstance)method).GenericArguments.ToArray());
                    }

                    inst.Operand = newMethod;

                    if (method.Name == "Create")
                    {
                        loadStateMachineInst = inst.Previous;
                        builderField = (FieldReference)inst.Next.Operand;
                    }
                }
            }

            var stateMachine = builderField.DeclaringType.Resolve();

            stateMachine.Fields.First(v => v.FieldType.IsTypeOf(asyncVoidMBType)).FieldType = asyncTaskMBType;

            foreach (var md in stateMachine.Methods)
                RewriteMethod(md);

            var getTask = asyncTaskMBType.Resolve().Properties.First(p => p.Name == "Task").GetMethod;

            if (loadStateMachineInst == null || builderField == null)
                throw new NotSupportedException("Unsupported state machine implementation");

            asyncVoidPc.InsertBefore(asyncVoidPc.CreateInstruction(loadStateMachineInst.OpCode, (VariableDefinition)loadStateMachineInst.Operand));
            asyncVoidPc.InsertBefore(asyncVoidPc.CreateInstruction(OpCodes.Ldflda, builderField));

            asyncVoidPc.InjectMethodCall(getTask, new object[] { });
            asyncVoidPc.SetVariableFromStack(taskVar);

            asyncVoidMethod.Body.OptimizeMacros();
        }

        private static void RewriteMethod(MethodDefinition method)
        {
            var asyncVoidMBType = method.Module.Import(typeof(AsyncVoidMethodBuilder));
            var asyncTaskMBType = method.Module.Import(typeof(AsyncTaskMethodBuilder));

            foreach (var inst in method.Body.Instructions)
            {
                var methodRef = inst.Operand as MethodReference;

                if (methodRef != null && methodRef.DeclaringType.IsTypeOf(asyncVoidMBType))
                {
                    var newMethod = method.Module.Import(asyncTaskMBType.Resolve().Methods.First(m => m.Name == methodRef.Name));

                    if (methodRef.IsGenericInstance)
                    {
                        newMethod = newMethod.MakeGeneric(newMethod.DeclaringType, ((IGenericInstance)methodRef).GenericArguments.ToArray());
                    }

                    inst.Operand = newMethod;
                }
            }
        }
    }
}