using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            Processor = TargetMethod.Body.GetILProcessor();

            SetupEntryPoints();
            SetupReturnPoints();
        }

        public ILProcessor Processor { get; private set; }

        public Instruction EntryPoint { get; private set; }

        public Instruction OriginalEntryPoint { get; private set; }

        public Instruction OriginalCodeReturnPoint { get; private set; }

        public Instruction ReturnPoint { get; private set; }

        public Instruction ExitPoint { get; private set; }

        public MethodDefinition TargetMethod { get; private set; }

        private void SetupEntryPoints()
        {
            OriginalEntryPoint = TargetMethod.IsConstructor ?
                TargetMethod.Body.Instructions.Skip(2).First() :
                TargetMethod.Body.Instructions.First();

            EntryPoint = Processor.SafeInsertBefore(OriginalEntryPoint, Processor.Create(OpCodes.Nop));
        }

        private void SetupReturnPoints()
        {
            ReturnPoint = Processor.Create(OpCodes.Nop);
            OriginalCodeReturnPoint = SetupSingleReturnPoint(Processor.Create(OpCodes.Br_S, ReturnPoint));

            Processor.SafeAppend(ReturnPoint);
            ExitPoint = Processor.SafeAppend(Processor.Create(OpCodes.Ret));
        }

        private Instruction SetupSingleReturnPoint(Instruction singleReturnPoint)
        {
            var rets = Processor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();

            if (rets.Count == 1)
                return Processor.SafeReplace(rets.First(), singleReturnPoint);//todo:: optimize, may fails on large methods

            foreach (var i in rets)
                Processor.SafeReplace(i, Processor.Create(OpCodes.Br_S, singleReturnPoint)); //todo:: optimize, may fails on large methods

            if (!TargetMethod.ReturnType.IsTypeOf(TargetMethod.Module.TypeSystem.Void))
            {
                if (TargetMethod.ReturnType.IsValueType)
                    Processor.SafeAppend(Processor.Create(OpCodes.Ldc_I4_0));
                else
                    Processor.SafeAppend(Processor.Create(OpCodes.Ldnull));
            }

            Processor.SafeAppend(singleReturnPoint);

            return singleReturnPoint;
        }

        private void SetupCatchBlock()
        {
        }
    }
}