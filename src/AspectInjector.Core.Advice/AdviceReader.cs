using System.Collections.Generic;
using AspectInjector.Core.Contracts;
using Mono.Cecil;
using AspectInjector.Core.Advice.Effects;
using System.Linq;
using AspectInjector.Core.Extensions;
using static AspectInjector.Broker.Advice;
using AspectInjector.Core.Models;

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
            var source = host as MethodDefinition;

            if (source != null)
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
                    var adviceType = ca.GetConstructorValue<Broker.Advice.Type>(0);
                    var advice = CreateEffect(adviceType);
                    if (advice == null)
                    {
                        _log.LogError(CompilationMessage.From($"Unknown advice type {adviceType.ToString()}", method));
                        continue;
                    }

                    advice.Method = method;
                    advice.Target = ca.GetConstructorValue<Target>(1);
                    advice.Arguments = ExtractArguments(method);

                    advices.Add(advice);
                }
            }

            return advices;
        }

        private List<AdviceArgument> ExtractArguments(MethodDefinition method)
        {
            var args = new List<AdviceArgument>();

            foreach (var par in method.Parameters)
            {
                var argAttr = par.CustomAttributes.FirstOrDefault(ca => ca.AttributeType.FullName == WellKnownTypes.Argument);

                if (argAttr == null)
                {
                    _log.LogError(CompilationMessage.From("Unbound arguments are not supported.", method));
                    continue;
                }

                args.Add(new AdviceArgument
                {
                    Source = argAttr.GetConstructorValue<Argument.Source>(0),
                    Parameter = par
                });
            }

            return args;
        }

        internal static AdviceEffectBase CreateEffect(Broker.Advice.Type adviceType)
        {
            switch (adviceType)
            {
                case Broker.Advice.Type.After: return new AfterAdviceEffect();
                case Broker.Advice.Type.Before: return new BeforeAdviceEffect();
                case Broker.Advice.Type.Around: return new AroundAdviceEffect();
                default: return null;
            }
        }
    }
}