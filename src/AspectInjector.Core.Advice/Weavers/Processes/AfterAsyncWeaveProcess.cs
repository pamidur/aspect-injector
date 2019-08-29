using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterAsyncWeaveProcess : AfterStateMachineWeaveProcessBase
    {
        private static readonly TypeReference _asyncStateMachineAttribute = StandardTypes.GetType(typeof(AsyncStateMachineAttribute));

        private readonly FieldDefinition _builderField;
        private readonly TypeReference _builder;
        private readonly TypeReference _asyncResult;

        public AfterAsyncWeaveProcess(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _builderField = _stateMachine.Fields.First(f => f.Name == "<>t__builder");
            _builder = _builderField.FieldType;
            _asyncResult = (_builder as IGenericInstance)?.GenericArguments.FirstOrDefault();
        }

        protected override TypeReference GetStateMachine()
        {
            var smRef = _target.CustomAttributes.First(ca => ca.AttributeType.Match(_asyncStateMachineAttribute))
                .GetConstructorValue<TypeReference>(0);

            if (smRef.HasGenericParameters)
            {
                var smDef = smRef.Resolve();
                smRef = ((MethodReference)_target.Body.Instructions
                    .First(i => i.OpCode == OpCodes.Newobj && i.Operand is MemberReference mr && mr.DeclaringType.Resolve() == smDef).Operand).DeclaringType;
            }

            return smRef;
        }

        protected override MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _stateMachine.Module.ImportReference(StandardTypes.Void));
                afterMethod.Parameters.Add(new ParameterDefinition(_stateMachine.Module.ImportReference(StandardTypes.Object)));

                _stateMachine.Methods.Add(afterMethod);

                afterMethod.Mark(WellKnownTypes.DebuggerHiddenAttribute);
                afterMethod.Body.Instead(pc => pc.Return());

                VariableDefinition resvar = null;

                if (_asyncResult != null)
                {
                    resvar = new VariableDefinition(_asyncResult);
                    moveNext.Body.Variables.Add(resvar);
                    moveNext.Body.InitLocals = true;
                }

                var setResultCall = _builder.Resolve().Methods.First(m => m.Name == "SetResult");
                var setResultCallRef = _asyncResult == null ? setResultCall : setResultCall.MakeGenericReference(_builder);

                moveNext.Body.OnCall(setResultCallRef, il =>
                {
                    il = il.Prev();

                    var loadArg = new PointCut(args => args.Value(null));

                    if (_asyncResult != null)
                    {
                        il = il.Store(resvar);
                        loadArg = new PointCut(args => args.Load(resvar).Cast(resvar.VariableType, StandardTypes.Object));
                    }

                    il = il.ThisOrStatic().Call(afterMethod.MakeGenericReference(_stateMachine), loadArg);

                    if (_asyncResult != null)
                        il = il.Load(resvar);

                    return il;
                });
            }

            return afterMethod;
        }

        protected override Cut LoadReturnValueArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.Load(pc.Method.Parameters[0]);
        }

        protected override Cut LoadReturnTypeArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_asyncResult ?? StandardTypes.Void);
        }

        protected override void InsertStateMachineCall(PointCut code)
        {
            var method = _builder.Resolve().Methods.First(m => m.Name == "Start").MakeGenericReference(_builder);

            var v = _target.Body.Variables.First(vr => vr.VariableType.Resolve().Match(_stateMachine));
            var loadVar = v.VariableType.IsValueType ? (PointCut)(c => c.LoadRef(v)) : c => c.Load(v);

            _target.Body.OnCall(method, cut => cut.Prev().Prev().Prev().Here(loadVar).Here(code));
        }
    }
}