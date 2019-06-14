using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Advice
{
    public class AdviceReader : IEffectReader
    {
        private readonly ILogger _log;

        public AdviceReader(ILogger log)
        {
            _log = log;
        }

        public IReadOnlyCollection<Effect> Read(ICustomAttributeProvider host)
        {
            if (host is MethodDefinition source)
                return Extract(source);

            return new List<Effect>();
        }

        private IReadOnlyCollection<AdviceEffectBase> Extract(MethodDefinition method)
        {
            var advices = new List<AdviceEffectBase>();

            foreach (var ca in method.CustomAttributes.ToList())
            {
                if (ca.AttributeType.FullName == WellKnownTypes.Advice)
                {
                    var adviceType = ca.GetConstructorValue<Kind>(0);
                    var advice = CreateEffect(adviceType);
                    if (advice == null)
                    {
                        _log.Log(GeneralRules.UnknownCompilationOption, method, GeneralRules.Literals.UnknownAdviceKind(adviceType.ToString()));
                        continue;
                    }

                    advice.Method = method;
                    advice.Target = ca.GetPropertyValue<Target>(nameof(Broker.Advice.Targets));

                    if (advice.Target > Target.Any)
                        _log.Log(GeneralRules.UnknownCompilationOption, method, GeneralRules.Literals.UnknownAdviceTarget(advice.Target.ToString()));

                    if ((advice.Target & Target.AnyScope) == 0) advice.Target ^= Target.AnyScope;
                    if ((advice.Target & Target.AnyMember) == 0) advice.Target ^= Target.AnyMember;
                    if ((advice.Target & Target.AnyAccess) == 0) advice.Target ^= Target.AnyAccess;

                    advice.Arguments = ExtractArguments(method);

                    advices.Add(advice);
                }
            }

            return advices;
        }

        private List<AdviceArgument> ExtractArguments(MethodDefinition method)
        {
            var args = new List<AdviceArgument>();

            foreach (var param in method.Parameters)
            {
                var argAttr = param.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == WellKnownTypes.Argument);
                if (argAttr == null)
                    _log.Log(EffectRules.AdviceArgumentMustBeBound, method, param.Name);

                var source = argAttr.GetConstructorValue<Source>(0);
                
                switch (source)
                {
                    case Source.Arguments:
                        if (!param.ParameterType.Match(StandardTypes.ObjectArray))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.ObjectArray);
                        break;
                    case Source.Instance:
                        if (!param.ParameterType.Match(StandardTypes.Object))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.Object);
                        break;
                    case Source.Metadata:
                        if (!param.ParameterType.Match(WellKnownTypes.MethodBase))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.MethodBase);
                        break;
                    case Source.Name:
                        if (!param.ParameterType.Match(StandardTypes.String))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.String);
                        break;
                    case Source.ReturnType:
                        if (!param.ParameterType.Match(StandardTypes.Type))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.Type);
                        break;
                    case Source.ReturnValue:
                        if (!param.ParameterType.Match(StandardTypes.Object))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.Object);
                        break;
                    case Source.Type:
                        if (!param.ParameterType.Match(StandardTypes.Type))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.Type);
                        break;
                    case Source.Triggers:
                        if (!param.ParameterType.Match(WellKnownTypes.AttributeArray))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.AttributeArray);
                        break;
                    case Source.Target:
                        if (!param.ParameterType.Match(WellKnownTypes.Func_ObjectArray_Object))
                            _log.Log(EffectRules.ArgumentMustHaveValidType, method, param.Name, EffectRules.Literals.TargetFunc);
                        break;
                    default:
                        _log.Log(GeneralRules.UnknownCompilationOption, method, GeneralRules.Literals.UnknownArgumentSource(source.ToString()));
                        break;
                }

                args.Add(new AdviceArgument
                {
                    Source = argAttr.GetConstructorValue<Source>(0),
                    Parameter = param
                });
            }

            return args;
        }

        internal static AdviceEffectBase CreateEffect(Kind adviceType)
        {
            switch (adviceType)
            {
                case Kind.After: return new AfterAdviceEffect();
                case Kind.Before: return new BeforeAdviceEffect();
                case Kind.Around: return new AroundAdviceEffect();
                default: return null;
            }
        }
    }
}