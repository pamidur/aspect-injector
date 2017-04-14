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
    internal class AfterAsyncWeaveProcess : AfterStateMachineWeaveProcessBase
    {
        private static readonly Type[] _supportedMethodBuilders = new[] {
            typeof(AsyncTaskMethodBuilder),
            typeof(AsyncTaskMethodBuilder<>),
            typeof(AsyncVoidMethodBuilder)
        };
        
        private readonly TypeReference _asyncResult;

        public AfterAsyncWeaveProcess(ILogger log, MethodDefinition target, AfterAdviceEffect effect, AspectDefinition aspect) : base(log, target, effect, aspect)
        {
            _asyncResult = (_stateMachine.Fields.First(f => f.Name == "<>t__builder").FieldType as IGenericInstance)?.GenericArguments.FirstOrDefault();
         }

        protected override TypeDefinition GetStateMachine()
        {
            return _target.CustomAttributes.First(ca => ca.AttributeType.IsTypeOf(typeof(AsyncStateMachineAttribute)))
                .GetConstructorValue<TypeReference>(0).Resolve();
        }


        protected override FieldReference FindField(ParameterDefinition p)
        {
            return _stateMachine.Fields.First(f => f.IsPublic && f.Name == p.Name);
        }

        protected override MethodDefinition FindOrCreateAfterStateMachineMethod()
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

                        il.ThisOrStatic().Call(afterMethod.MakeHostInstanceGeneric(_stateMachine), loadArg);

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

        protected override void LoadReturnTypeArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.TypeOf(_asyncResult ?? _ts.Void);
        }

        protected override void InsertStateMachineCall(Action<PointCut> code)
        {
            var tgti = _target.Body.Instructions.First(i => i.OpCode == OpCodes.Stloc_0);
            _target.GetEditor().OnInstruction(tgti, code);
        }
    }
}