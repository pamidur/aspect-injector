using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.BuildTask
{
    public class TargetMethodContext
    {
        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            Processor = TargetMethod.Body.GetILProcessor();
            OriginalEntryPoint = TargetMethod.Body.Instructions.First();

            if (TargetMethod.Body.Instructions.All(i => i.OpCode != OpCodes.Ret))
                Processor.Append(Processor.Create(OpCodes.Ret));

            OriginalReturnPoint = TargetMethod.Body.Instructions.Single(i => i.OpCode == OpCodes.Ret);
        }

        public MethodDefinition TargetMethod { get; private set; }

        public ILProcessor Processor { get; private set; }

        public Instruction OriginalEntryPoint { get; private set; }

        public Instruction OriginalReturnPoint { get; private set; }

        private Instruction _returnPoint;

        public Instruction ReturnPoint
        {
            get
            {
                if (_returnPoint == null)
                {
                    _returnPoint = Processor.Create(OpCodes.Ret);
                    Processor.InsertAfter(OriginalReturnPoint, _returnPoint);
                    Processor.Replace(OriginalReturnPoint, Processor.Create(OpCodes.Nop));
                }

                return _returnPoint;
            }
        }
    }
}