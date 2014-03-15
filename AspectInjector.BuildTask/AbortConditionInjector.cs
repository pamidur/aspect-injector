using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;

namespace AspectInjector.BuildTask
{
    internal class AbortConditionInjector
    {
        private const string AbortFlagVariableName = "__a$_abortMethod";

        private readonly ILProcessor processor;
        private readonly MethodDefinition adviceMethod;
        private readonly MethodDefinition targetMethod;
        private readonly Instruction injectionPoint;
        private readonly Instruction returnPoint;

        public VariableDefinition AbortFlagVariable { get; private set; }
        public VariableDefinition ResultVariable { get; private set; }

        public AbortConditionInjector(ILProcessor processor,
            MethodDefinition adviceMethod,
            MethodDefinition targetMethod,
            Instruction injectionPoint,
            Instruction returnPoint)
        {
            this.processor = processor;
            this.adviceMethod = adviceMethod;
            this.targetMethod = targetMethod;
            this.injectionPoint = injectionPoint;
            this.returnPoint = returnPoint;
        }

        public void InjectVariables()
        {
            AbortFlagVariable = targetMethod.Body.Variables.SingleOrDefault(v => v.Name == AbortFlagVariableName);
            if (AbortFlagVariable == null)
            {
                AbortFlagVariable = processor.CreateLocalVariable(targetMethod,
                    injectionPoint,
                    targetMethod.Module.TypeSystem.Boolean,
                    false,
                    AbortFlagVariableName);
            }

            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                ResultVariable = processor.CreateLocalVariable<object>(targetMethod,
                    injectionPoint,
                    targetMethod.Module.Import(adviceMethod.ReturnType),
                    null);
            }
        }

        public void InjectStatements()
        {
            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Stloc, ResultVariable.Index));
            }

            processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, AbortFlagVariable.Index));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I4_1));

            var continuePoint = processor.Create(OpCodes.Nop);
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ceq));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Brfalse_S, continuePoint));

            if (!adviceMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, ResultVariable.Index));
            }

            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Br_S, returnPoint));
            processor.InsertBefore(injectionPoint, continuePoint);
        }
    }
}
