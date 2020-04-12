using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Advice.Weavers.Processes;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using System;

namespace AspectInjector.Core.Advice.Weavers
{
    public class AdviceInlineWeaver : IEffectWeaver
    {
        public virtual byte Priority => 90;

        protected readonly ILogger _log;

        public AdviceInlineWeaver(ILogger log)
        {
            _log = log;
        }

        public virtual bool CanWeave(InjectionDefinition injection)
        {
            var result =
                (injection.Effect is BeforeAdviceEffect || injection.Effect is AfterAdviceEffect) &&
                (injection.Target is EventDefinition || injection.Target is PropertyDefinition || injection.Target is MethodDefinition);

            if (result && injection.Target is MethodDefinition && injection.Effect is AfterAdviceEffect)
            {
                var md = (MethodDefinition)injection.Target;
                if (md.IsAsync() || md.IsIterator())
                    result = false;
            }

            return result;
        }

        public void Weave(InjectionDefinition injection)
        {
            var effect = (AdviceEffectBase)injection.Effect;

            if (injection.Target is EventDefinition eventDefinition)
            {
                if (eventDefinition.AddMethod != null && effect.Target.HasFlag(Target.EventAdd))
                    WeaveMethod(eventDefinition.AddMethod, injection);

                if (eventDefinition.RemoveMethod != null && effect.Target.HasFlag(Target.EventRemove))
                    WeaveMethod(eventDefinition.RemoveMethod, injection);
                return;
            }

            if (injection.Target is PropertyDefinition propertyDefinition)
            {
                if (propertyDefinition.SetMethod != null && effect.Target.HasFlag(Target.Setter))
                    WeaveMethod(propertyDefinition.SetMethod, injection);

                if (propertyDefinition.GetMethod != null && effect.Target.HasFlag(Target.Getter))
                    WeaveMethod(propertyDefinition.GetMethod, injection);

                return;
            }

            if (injection.Target is MethodDefinition methodDefinition)
            {
                if (methodDefinition.IsConstructor && effect.Target.HasFlag(Target.Constructor))
                    WeaveMethod(methodDefinition, injection);

                if (!methodDefinition.IsConstructor && effect.Target.HasFlag(Target.Method))
                    WeaveMethod(methodDefinition, injection);

                return;
            }

            _log.Log(GeneralRules.UnexpectedCompilerBehaviour, effect.Method, $"Unexpected injection target '{injection.Target}'");
        }

        protected virtual void WeaveMethod(MethodDefinition method, InjectionDefinition injection)
        {
            switch(injection.Effect)
            {
                case AfterAdviceEffect _:
                    {
                        var process = new AdviceAfterProcess(_log, method, injection);
                        process.Execute();
                        break;
                    }
                case BeforeAdviceEffect _:
                    {
                        var process = new AdviceBeforeProcess(_log, method, injection);
                        process.Execute();
                        break;
                    }
                case null: throw new ArgumentNullException(nameof(injection), "Effect is null.");
                default: throw new NotSupportedException($"Unknown effect type. {injection.Effect.GetType().Name}");
            }
        }
    }
}