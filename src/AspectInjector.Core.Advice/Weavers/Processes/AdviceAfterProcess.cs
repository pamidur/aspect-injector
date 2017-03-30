using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceAfterProcess : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        private VariableDefinition _retvar;

        public AdviceAfterProcess(ILogger log, MethodDefinition target, AspectDefinition aspect, AfterAdviceEffect effect)
            : base(log, target, effect, aspect)
        {
            if (!_target.ReturnType.IsTypeOf(_ts.Void) && effect.Arguments.Any(a => a.Source == Broker.Advice.Argument.Source.ReturnValue))
                _retvar = GetOrCreateRetVar();
        }

        private VariableDefinition GetOrCreateRetVar()
        {
            var ret = _target.Body.Variables.FirstOrDefault(v => v.Name == Constants.ReturnValueArgName);
            if (ret == null)
            {
                ret = new VariableDefinition(Constants.ReturnValueArgName, _target.ReturnType);
                _target.Body.Variables.Add(ret);
                _target.Body.InitLocals = true;
            }

            return ret;
        }

        public override void Execute()
        {
            _target.GetEditor().OnExit(
                il =>
                {
                    if (_retvar != null)
                        il.Store(_retvar);

                    il.LoadAspect(_aspect);
                    il.Call(_effect.Method, LoadAdviceArgs);

                    if (_retvar != null)
                        il.Load(_retvar);
                });
        }

        protected override void LoadReturnValueArgument(PointCut pc, AdviceArgument parameter)
        {
            if (_retvar == null)
                pc.Null();
            else
                pc.Load(_retvar).ByVal(_retvar.VariableType);
        }
    }
}