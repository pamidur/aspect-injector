﻿using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterAsyncWeaveProcess : AfterStateMachineWeaveProcessBase
    {
        private readonly TypeReference _builder;
        private readonly TypeReference _asyncResult;

        public AfterAsyncWeaveProcess(ILogger log, MethodDefinition target, InjectionDefinition injection)
            : base(log, target, injection)
        {
            SetStateMachine(GetStateMachine());

            var builderField = _stateMachine.Fields.First(f => f.Name == "<>t__builder" || f.Name == "$Builder");

            _builder = builderField.FieldType;
            _asyncResult = (_builder as IGenericInstance)?.GenericArguments.FirstOrDefault();
        }

        private TypeReference GetStateMachine()
        {
            var smRef = _method.CustomAttributes.First(ca => ca.AttributeType.Match(StandardType.AsyncStateMachineAttribute))
                .GetConstructorValue<TypeReference>(0);

            if (smRef.HasGenericParameters)
                smRef = _method.Body.Variables.First(v => v.VariableType.Resolve() == smRef).VariableType;

            return smRef;
        }

        protected override MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _stateMachine.Module.TypeSystem.Void);
                afterMethod.Parameters.Add(new ParameterDefinition(_stateMachine.Module.TypeSystem.Object));

                _stateMachine.Methods.Add(afterMethod);

                afterMethod.Mark(_module.ImportStandardType(WellKnownTypes.DebuggerHiddenAttribute));
                afterMethod.Body.Instead((in Cut pc) => pc.Return());

                VariableDefinition resvar = null;

                if (_asyncResult != null)
                {
                    resvar = new VariableDefinition(_asyncResult);
                    moveNext.Body.Variables.Add(resvar);
                    moveNext.Body.InitLocals = true;
                }

                var setResultCall = _builder.Resolve().Methods.First(m => m.Name == "SetResult");
                var setResultCallRef = _asyncResult == null ? setResultCall : setResultCall.MakeReference(_builder);

                moveNext.Body.OnCall(setResultCallRef, (in Cut oncall_cut) =>
                {
                    var il = oncall_cut.Prev();

                    var loadArg = new PointCut((in Cut args) => args.Value(null));

                    if (_asyncResult != null)
                    {
                        il = il.Store(resvar);
                        loadArg = new PointCut((in Cut args) => args.Load(resvar).Cast(resvar.VariableType, args.TypeSystem.Object));
                    }

                    il = il.ThisOrStatic().Call(afterMethod.MakeReference(_stateMachine.MakeSelfReference()), loadArg);

                    if (_asyncResult != null)
                        il = il.Load(resvar);

                    return il;
                });
            }

            return afterMethod;
        }

        protected override Cut LoadReturnValueArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.Load(pc.Method.Parameters[0]);
        }

        protected override Cut LoadReturnTypeArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_asyncResult ?? pc.TypeSystem.Void);
        }

        protected override void InsertStateMachineCall(PointCut code)
        {
            var smctor = _method.Body.Instructions.Where(i => i.OpCode == OpCodes.Newobj && i.Operand is MethodReference mr && mr.DeclaringType.Resolve() == _stateMachine).ToArray();

            if (smctor.Length > 0) // DEBUG build
            {
                _method.Body.AfterInstruction(smctor[0], (in Cut cut) => cut.Dup().Here(code));
            }
            else // RELEASE build
            {
                var v = _method.Body.Variables.First(vr => vr.VariableType.Resolve().Match(_stateMachine));
                var sminit = _method.Body.Instructions.Where(i => i.OpCode == OpCodes.Initobj && i.Operand is TypeReference tr && tr.Resolve() == _stateMachine).ToArray();

                if (sminit.Length > 0) // VB                                   
                    _method.Body.AfterInstruction(sminit[0], (in Cut cut) => cut.LoadRef(v).Here(code));
                else // C#         
                    _method.Body.AfterEntry((in Cut cut) => cut.LoadRef(v).Here(code));

            }
        }
    }
}