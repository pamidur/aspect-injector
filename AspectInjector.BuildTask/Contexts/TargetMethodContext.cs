using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        private Instruction _returnPoint;
        private Instruction _originalReturnPoint;

        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            Processor = TargetMethod.Body.GetILProcessor();
            OriginalEntryPoint = TargetMethod.IsConstructor ?
                TargetMethod.Body.Instructions.Skip(2).First() :
                TargetMethod.Body.Instructions.First();

            EntryPoint = Processor.Create(OpCodes.Nop);
            Processor.InsertBefore(TargetMethod.IsConstructor ?
                TargetMethod.Body.Instructions.Skip(2).First() :
                TargetMethod.Body.Instructions.First(),
                        EntryPoint);
        }

        public Instruction OriginalEntryPoint { get; private set; }


        public Instruction OriginalReturnPoint
        {
            get
            {
                if (_originalReturnPoint == null)
                {
                    if (TargetMethod.Body.Instructions.All(i => i.OpCode != OpCodes.Ret))
                    {
                        if (!TargetMethod.ReturnType.IsTypeOf(TargetMethod.Module.TypeSystem.Void))
                        {
                            if (TargetMethod.ReturnType.IsValueType)
                                Processor.Append(Processor.Create(OpCodes.Ldc_I4_0));
                            else
                                Processor.Append(Processor.Create(OpCodes.Ldnull));
                        }

                        Processor.Append(Processor.Create(OpCodes.Ret));
                    }

                    _originalReturnPoint = TargetMethod.Body.Instructions.Single(i => i.OpCode == OpCodes.Ret);
                }

                return _originalReturnPoint;
            }
        }


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

        public Instruction EntryPoint { get; private set; }
        public MethodDefinition TargetMethod { get; private set; }
    }
}