using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using static AspectInjector.Broker.Advice;
using System;
using System.Linq;

namespace AspectInjector.Core.Advice.Effects
{
    internal abstract class AdviceEffectBase : Effect
    {
        public Target Target { get; set; }
        public abstract Broker.Advice.Type Type { get; }
        public MethodDefinition Method { get; set; }

        public List<AdviceArgument> Arguments { get; set; } = new List<AdviceArgument>();

        public override bool IsApplicableFor(ICustomAttributeProvider target)
        {
            if (Target.HasFlag(Target.Method) && target is MethodDefinition && ((MethodDefinition)target).IsNormalMethod())
                return true;

            if (Target.HasFlag(Target.Constructor) && target is MethodDefinition && ((MethodDefinition)target).IsConstructor)
                return true;

            if (Target.HasFlag(Target.Setter) && target is PropertyDefinition && ((PropertyDefinition)target).SetMethod != null)
                return true;

            if (Target.HasFlag(Target.Getter) && target is PropertyDefinition && ((PropertyDefinition)target).GetMethod != null)
                return true;

            if (Target.HasFlag(Target.EventAdd) && target is EventDefinition && ((EventDefinition)target).AddMethod != null)
                return true;

            if (Target.HasFlag(Target.EventRemove) && target is EventDefinition && ((EventDefinition)target).RemoveMethod != null)
                return true;

            return false;
        }

        protected override bool IsEqualTo(Effect effect)
        {
            var other = effect as AdviceEffectBase;

            if (other == null)
                return false;

            return other.Target == Target && other.Type == Type && other.Method == Method;
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            if (Method.IsStatic)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} cannot be static.", aspect.Host));
                return false;
            }

            if (!Method.IsPublic)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} should be public.", aspect.Host));
                return false;
            }

            return true;
        }
    }
}