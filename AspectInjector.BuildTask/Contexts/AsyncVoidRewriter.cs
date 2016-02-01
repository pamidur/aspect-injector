using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.BuildTask.Contexts
{
    internal static class AsyncVoidRewriter
    {
        public static void Rewrite(PointCut asyncVoidPc, MethodDefinition asyncVoidMethod, VariableDefinition taskVar)
        {
            var asyncVoidMBType = asyncVoidMethod.Module.Import(typeof(AsyncVoidMethodBuilder));
            var asyncTaskMBType = asyncVoidMethod.Module.Import(typeof(AsyncTaskMethodBuilder));

            asyncVoidMethod.Body.Variables.First(v => v.VariableType.IsTypeOf(asyncVoidMBType)).VariableType = asyncTaskMBType;

            OpCode? loadStateMachineCode = null;
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
                        loadStateMachineCode = inst.Previous.OpCode;
                        builderField = (FieldReference)inst.Next.Operand;
                    }
                }
            }

            var stateMachine = builderField.DeclaringType.Resolve();

            stateMachine.Fields.First(v => v.FieldType.IsTypeOf(asyncVoidMBType)).FieldType = asyncTaskMBType;

            foreach (var inst in stateMachine.Methods.First(m => m.Name == "MoveNext").Body.Instructions)
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
                }
            }

            var getTask = asyncTaskMBType.Resolve().Properties.First(p => p.Name == "Task").GetMethod;

            //should look into attribute for state machine type
            asyncVoidPc.InsertBefore(asyncVoidPc.CreateInstruction(loadStateMachineCode.Value)); //replace with state machine var
            asyncVoidPc.InsertBefore(asyncVoidPc.CreateInstruction(OpCodes.Ldflda, builderField));

            asyncVoidPc.InjectMethodCall(getTask, new object[] { });
            asyncVoidPc.SetVariableFromStack(taskVar);
        }
    }
}