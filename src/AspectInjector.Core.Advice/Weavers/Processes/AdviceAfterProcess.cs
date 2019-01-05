using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceAfterProcess : AdviceWeaveProcessBase<AfterAdviceEffect>
    {
        private static readonly Dictionary<MethodBody, VariableDefinition> _returnVariablesCache = new Dictionary<MethodBody, VariableDefinition>();

        private VariableDefinition _retvar;

        public AdviceAfterProcess(ILogger log, MethodDefinition target, InjectionDefinition injection)
            : base(log, target, injection)
        {
            if (_target.ReturnType.FullName != WellKnownTypes.Void && _effect.Arguments.Any(a => a.Source == Source.ReturnValue))
                _retvar = GetOrCreateRetVar();
        }

        private VariableDefinition GetOrCreateRetVar()
        {
            if (!_returnVariablesCache.TryGetValue(_target.Body, out var ret))
            {
                ret = new VariableDefinition(_target.ReturnType);
                _target.Body.Variables.Add(ret);
                _target.Body.InitLocals = true;

                _returnVariablesCache.Add(_target.Body, ret);
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
                pc.Load(_retvar).Cast(_retvar.VariableType, _ts.Object);
        }
    }
}