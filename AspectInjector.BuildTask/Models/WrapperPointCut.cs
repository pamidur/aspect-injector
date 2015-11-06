using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask.Models
{
    internal class WrapperPointCut : PointCut
    {
        #region Fields

        private readonly ParameterDefinition _assembledArgsParam;

        #endregion Fields

        #region Constructors

        public WrapperPointCut(ParameterDefinition assembledArgsParam, ILProcessor processor, Instruction instruction)
            : base(processor, instruction)
        {
            _assembledArgsParam = assembledArgsParam;
        }

        #endregion Constructors

        #region Methods

        public override PointCut CreatePointCut(Instruction instruction)
        {
            return new WrapperPointCut(_assembledArgsParam, Processor, instruction);
        }

        //todo:: optimize for full args load as object[]
        public override void LoadParameterOntoStack(ParameterDefinition parameter, TypeReference expectedType = null)
        {
            LoadSelfOntoStack();
            LoadParameterOntoStack(_assembledArgsParam);

            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, parameter.Index));
            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldelem_Ref));

            BoxUnboxIfNeeded(parameter.ParameterType, expectedType);
        }

        #endregion Methods
    }
}