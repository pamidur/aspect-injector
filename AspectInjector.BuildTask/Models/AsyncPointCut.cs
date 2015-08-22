using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask.Models
{
    internal class AsyncPointCut : PointCut
    {
        private FieldReference _originalTypeRef;

        public AsyncPointCut(FieldReference originalTypeRef, ILProcessor processor, Instruction instruction)
            : base(processor, instruction)
        {
            _originalTypeRef = (FieldReference)CreateMemberReference(originalTypeRef);
        }

        public override void LoadAspectInstance(FieldReference field)
        {
            Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldarg_0));
            Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldfld, _originalTypeRef));

            var fieldRef = (FieldReference)CreateMemberReference(field);

            if (field.Resolve().IsStatic)
            {
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldsfld, fieldRef));
            }
            else
            {
                Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldfld, fieldRef));
            }
        }

        public override PointCut CreatePointCut(Instruction instruction)
        {
            return new AsyncPointCut(_originalTypeRef, Processor, instruction);
        }
    }
}