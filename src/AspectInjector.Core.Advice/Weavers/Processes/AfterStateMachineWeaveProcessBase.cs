﻿using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AfterStateMachineWeaveProcessBase : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        protected TypeDefinition _stateMachine;
        protected TypeReference _stateMachineRef;

        private readonly Func<FieldReference> _originalThis;

        protected AfterStateMachineWeaveProcessBase(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _originalThis = _method.IsStatic ? (Func<FieldReference>)null : () => GetThisField();
        }

        protected void SetStateMachine(TypeReference stateMachineRef)
        {
            _stateMachineRef = stateMachineRef;
            _stateMachine = _stateMachineRef.Resolve();
        }

        private FieldReference GetThisField()
        {
            var thisfield = _stateMachine.Fields
                .FirstOrDefault(f => f.Name == Constants.MovedThis);

            if (thisfield == null)
            {
                TypeReference _origTypeRef = _type;
                if (_origTypeRef.HasGenericParameters)
                    _origTypeRef = _origTypeRef.MakeGenericInstanceType(_stateMachine.GenericParameters.Take(_type.GenericParameters.Count).ToArray());

                thisfield = new FieldDefinition(Constants.MovedThis, FieldAttributes.Public, _origTypeRef);
                _stateMachine.Fields.Add(thisfield);

                InsertStateMachineCall(
                    (in Cut e) => e
                    .Store(thisfield.MakeReference(_stateMachineRef), (in Cut v) => v.This())
                    );
            }

            return thisfield.MakeReference(_stateMachine.MakeSelfReference());
        }

        private FieldReference GetArgsField()
        {
            var argsfield = _stateMachine.Fields
                .FirstOrDefault(f => f.Name == Constants.MovedArgs);

            if (argsfield == null)
            {
                argsfield = new FieldDefinition(Constants.MovedArgs, FieldAttributes.Public, _stateMachine.Module.ImportStandardType(WellKnownTypes.Object_Array));
                _stateMachine.Fields.Add(argsfield);

                InsertStateMachineCall(
                    (in Cut e) => e
                    .Store(argsfield.MakeReference(_stateMachineRef), (in Cut v) =>
                    {
                        var elements = _method.Parameters.Select<ParameterDefinition, PointCut>(p => (in Cut il) =>
                               il.Load(p).Cast(p.ParameterType, il.TypeSystem.Object)
                           ).ToArray();

                        return v.CreateArray(v.TypeSystem.Object, elements);
                    }));
            }

            return argsfield.MakeReference(_stateMachine.MakeSelfReference());
        }

        protected abstract void InsertStateMachineCall(PointCut code);

        public override void Execute()
        {
            if (_stateMachineRef == null)
                throw new InvalidOperationException("State machine is not set");

            FindOrCreateAfterStateMachineMethod().Body.BeforeExit(
                (in Cut e) => e
                .LoadAspect(_aspect, _method, LoadOriginalThis)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        protected Cut LoadOriginalThis(in Cut pc)
        {
            return _originalThis == null ? pc : pc.This().Load(_originalThis());
        }

        protected override Cut LoadInstanceArgument(in Cut pc, AdviceArgument parameter)
        {
            if (_originalThis != null)
                return LoadOriginalThis(pc);
            else
                return pc.Value(null);
        }

        protected override Cut LoadArgumentsArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.This().Load(GetArgsField());
        }

        protected abstract MethodDefinition FindOrCreateAfterStateMachineMethod();
    }
}
