using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
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
            if (!_target.ReturnType.Match(StandardTypes.Void) && _effect.Arguments.Any(a => a.Source == Source.ReturnValue))
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
            _target.Body.BeforeExit(
                cut =>
                {
                    if (_retvar != null)
                        cut = cut.Store(_retvar);

                    cut = cut
                    .LoadAspect(_aspect)
                    .Call(_effect.Method, LoadAdviceArgs);

                    if (_retvar != null)
                        cut = cut.Load(_retvar);

                    return cut;
                });
        }

        protected override Cut LoadReturnValueArgument(Cut pc, AdviceArgument parameter)
        {
            if (_retvar == null)
                return pc.Null();
            else
                return pc.Load(_retvar).Cast(_retvar.VariableType, StandardTypes.Object);
        }
    }
}