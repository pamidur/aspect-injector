using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        private Instruction _returnPoint;
        private Instruction _entryPoint;

        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            Processor = TargetMethod.Body.GetILProcessor();
            OriginalEntryPoint = TargetMethod.Body.Instructions.First();

            if (TargetMethod.Body.Instructions.All(i => i.OpCode != OpCodes.Ret))
                Processor.Append(Processor.Create(OpCodes.Ret));

            OriginalReturnPoint = TargetMethod.Body.Instructions.Single(i => i.OpCode == OpCodes.Ret);
        }

        public Instruction OriginalEntryPoint { get; private set; }
        public Instruction OriginalReturnPoint { get; private set; }
        public ILProcessor Processor { get; private set; }
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

        public Instruction EntryPoint
        {
            get
            {
                if (_entryPoint == null)
                {
                    _entryPoint = Processor.Create(OpCodes.Nop);
                    Processor.InsertBefore(Processor.Body.Instructions.First(), _entryPoint);
                }

                return _entryPoint;
            }
        }
        public MethodDefinition TargetMethod { get; private set; }
    }
}