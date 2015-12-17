using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask.Models
{
    internal class WrapperPointCut : PointCut
    {
        #region Fields

        private readonly ParameterDefinition _assembledArgsParam;
        private readonly MethodDefinition _nextWrapper;

        #endregion Fields

        #region Constructors

        public WrapperPointCut(ParameterDefinition assembledArgsParam, MethodDefinition nextWrapper, ILProcessor processor, Instruction instruction)
            : base(processor, instruction)
        {
            _assembledArgsParam = assembledArgsParam;
            _nextWrapper = nextWrapper;
        }

        #endregion Constructors

        #region Methods

        public override PointCut CreatePointCut(Instruction instruction)
        {
            return new WrapperPointCut(_assembledArgsParam, _nextWrapper, Processor, instruction);
        }

        public override void LoadParameterOntoStack(ParameterDefinition parameter, TypeReference expectedType = null)
        {
            LoadSelfOntoStack();
            base.LoadParameterOntoStack(_assembledArgsParam);

            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldc_I4, parameter.Index));
            Processor.InsertBefore(InjectionPoint, CreateInstruction(OpCodes.Ldelem_Ref));

            BoxUnboxIfNeeded(parameter.ParameterType, expectedType);
        }

        public override void LoadAllArgumentsOntoStack()
        {
            base.LoadParameterOntoStack(_assembledArgsParam);
        }

        protected override void LoadTargetFunc()
        {
            //var func = new Func<object[], object>(this.< UnwrapFunction >);
            var targetFuncType = TypeSystem.MakeGenericInstanceType(
                TypeSystem.FuncGeneric2,
                TypeSystem.MakeArrayType(TypeSystem.Object),
                TypeSystem.Object);

            var targetFuncCtor = ModuleContext.ModuleDefinition.Import(targetFuncType.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic))
               .MakeGeneric(targetFuncType, TypeSystem.MakeArrayType(TypeSystem.Object), TypeSystem.Object);

            LoadSelfOntoStack();
            InsertBefore(CreateInstruction(OpCodes.Ldftn, _nextWrapper));
            InsertBefore(CreateInstruction(OpCodes.Newobj, (MethodReference)CreateMemberReference(targetFuncCtor)));
        }

        #endregion Methods
    }
}