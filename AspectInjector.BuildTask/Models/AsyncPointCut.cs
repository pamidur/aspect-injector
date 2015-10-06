using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask.Models
{
    internal class AsyncPointCut : PointCut
    {
        #region Fields

        private readonly FieldReference _methodArgsRef;
        private readonly FieldReference _originalTypeRef;

        #endregion Fields

        #region Constructors

        public AsyncPointCut(FieldReference originalTypeRef, FieldReference methodArgsRef, ILProcessor processor, Instruction instruction)
            : base(processor, instruction)
        {
            _originalTypeRef = (FieldReference)CreateMemberReference(originalTypeRef);
            _methodArgsRef = (FieldReference)CreateMemberReference(methodArgsRef);
        }

        #endregion Constructors

        #region Methods

        public override PointCut CreatePointCut(Instruction instruction)
        {
            return new AsyncPointCut(_originalTypeRef, _methodArgsRef, Processor, instruction);
        }

        public override void LoadFieldOntoStack(FieldReference field)
        {
            base.LoadFieldOntoStack(_originalTypeRef);

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

        public override void LoadParameterOntoStack(ParameterDefinition parameter, TypeReference expectedType)
        {
            base.LoadFieldOntoStack(_methodArgsRef);

            var module = Processor.Body.Method.Module;

            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, parameter.Index));
            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldelem_Ref));

            //if (parameter.ParameterType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
            //    Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Box, module.Import(parameter.ParameterType)));
        }

        #endregion Methods
    }
}