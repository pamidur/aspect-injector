using AspectInjector.Core.Advice.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using Mono.Cecil.Cil;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AfterStateMachineWeaveProcessBase : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        protected readonly FieldDefinition _originalThis;
        protected readonly TypeDefinition _stateMachine;

        public AfterStateMachineWeaveProcessBase(ILogger log, MethodDefinition target, AfterAdviceEffect effect, AspectDefinition aspect) : base(log, target, effect, aspect)
        {
            _stateMachine = GetStateMachine();
            _originalThis = _target.IsStatic ? null : GetThisField();
        }

        protected abstract TypeDefinition GetStateMachine();

        private FieldDefinition GetThisField()
        {
            var thisfield = _stateMachine.Fields.FirstOrDefault(f => f.Name == Constants.MovedThis);

            if (thisfield == null)
            {
                thisfield = new FieldDefinition(Constants.MovedThis, FieldAttributes.Public, _stateMachine.MakeCallReference(_stateMachine.DeclaringType));
                _stateMachine.Fields.Add(thisfield);

                InsertStateMachineCall(
                    e => e
                    .Dup()
                    .Store(thisfield, v => v.This())
                    );
            }

            return thisfield;
        }

        protected abstract void InsertStateMachineCall(Action<PointCut> code);

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().GetEditor().OnExit(
                e => e
                .LoadAspect(_aspect, _target, LoadOriginalThis, _target.DeclaringType)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        protected void LoadOriginalThis(PointCut pc)
        {
            if (_originalThis != null)
                pc.This().Load(_originalThis);
        }

        protected override void LoadInstanceArgument(PointCut pc, AdviceArgument parameter)
        {
            if (_originalThis != null)
                LoadOriginalThis(pc);
            else
                pc.Value((object)null);
        }

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            var elements = _target.Parameters.Select<ParameterDefinition, Action<PointCut>>(p => il =>
            {
                var field = FindField(p);
                il.ThisOrStatic().Load(field).ByVal(field.FieldType);
            }
            ).ToArray();

            pc.CreateArray<object>(elements);
        }

        protected abstract FieldReference FindField(ParameterDefinition p);
        protected abstract MethodDefinition FindOrCreateAfterStateMachineMethod();
    }
}
