using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AfterAsyncWeaveProcess : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        private static readonly Type[] _supportedMethodBuilders = new[] {
            typeof(AsyncTaskMethodBuilder),
            typeof(AsyncTaskMethodBuilder<>),
            typeof(AsyncVoidMethodBuilder)
        };

        private readonly TypeDefinition _stateMachine;
        private readonly FieldDefinition _this;
        private readonly TypeReference _asyncResult;

        public AfterAsyncWeaveProcess(ILogger log, MethodDefinition target, AfterAdviceEffect effect, AspectDefinition aspect) : base(log, target, effect, aspect)
        {
            _stateMachine = target.CustomAttributes.First(ca => ca.AttributeType.IsTypeOf(typeof(AsyncStateMachineAttribute)))
                .GetConstructorValue<TypeReference>(0).Resolve();

            _asyncResult = _target.ReturnType.IsTypeOf(_ts.Void) ? null : (_target.ReturnType as IGenericInstance)?.GenericArguments.FirstOrDefault();

            _this = _target.IsStatic ? null : _stateMachine.Fields.First(f => f.IsPublic && f.FieldType.IsTypeOf(_target.DeclaringType) && f.Name.StartsWith("<>") && f.Name.EndsWith("_this"));
        }

        public override void Execute()
        {
            FindOrCreateAfterStateMachineMethod().GetEditor().OnExit(
                e => e
                .LoadAspect(_aspect, LoadThis, _target.DeclaringType)
                .Call(_effect.Method, LoadAdviceArgs)
            );
        }

        private void LoadThis(PointCut pc)
        {
            if (_this != null)
                pc.This().Load(_this);
        }

        protected override void LoadInstanceArgument(PointCut pc, AdviceArgument parameter)
        {
            LoadThis(pc);
        }

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            var elements = _target.Parameters.Select<ParameterDefinition, Action<PointCut>>(p => il =>
                il.ThisOrStatic().Load(FindField(p)).ByVal(p.ParameterType)
            ).ToArray();

            pc.CreateArray<object>(elements);
        }

        private FieldReference FindField(ParameterDefinition p)
        {
            return _stateMachine.Fields.First(f => f.IsPublic && f.Name == p.Name);
        }

        protected MethodDefinition FindOrCreateAfterStateMachineMethod()
        {
            var afterMethod = _stateMachine.Methods.FirstOrDefault(m => m.Name == Constants.AfterStateMachineMethodName);

            if (afterMethod == null)
            {
                var moveNext = _stateMachine.Methods.First(m => m.Name == "MoveNext");

                var exitPoints = moveNext.Body.Instructions.Where(i =>
                {
                    if (i.OpCode != OpCodes.Call)
                        return false;

                    var method = ((MethodReference)i.Operand).Resolve();
                    return method.Name == "SetResult" && _supportedMethodBuilders.Any(bt => method.DeclaringType.IsTypeOf(bt));
                }).ToList();

                afterMethod = new MethodDefinition(Constants.AfterStateMachineMethodName, MethodAttributes.Private, _ts.Void);
                afterMethod.Parameters.Add(new ParameterDefinition(_ts.Object));

                _stateMachine.Methods.Add(afterMethod);

                afterMethod.GetEditor().Instead(pc => pc.Return());

                VariableDefinition resvar = null;

                if (_asyncResult != null)
                {
                    resvar = new VariableDefinition(_asyncResult);
                    moveNext.Body.Variables.Add(resvar);
                    moveNext.Body.InitLocals = true;
                }

                foreach (var exit in exitPoints)
                {
                    moveNext.GetEditor().OnInstruction(exit, il =>
                    {
                        var loadArg = new Action<PointCut>(args => args.Value((object)null));

                        if (_asyncResult != null)
                        {
                            il.Store(resvar);
                            loadArg = new Action<PointCut>(args => args.Load(resvar).ByVal(_asyncResult));
                        }

                        il.ThisOrStatic().Call(afterMethod, loadArg);

                        if (_asyncResult != null)
                            il.Load(resvar);
                    });
                }
            }

            return afterMethod;
        }

        protected override void LoadReturnValueArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.Load(pc.Method.Parameters[0]);
        }
    }
}