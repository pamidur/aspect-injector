using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;
using static AspectInjector.Broker.Advice;

namespace AspectInjector.Core.Advice.Effects
{
    internal abstract class AdviceEffectBase : Effect
    {
        public Target Target { get; set; }
        public AccessModifier WithAccess { get; set; }

        public MethodDefinition Method { get; set; }
        public List<AdviceArgument> Arguments { get; set; } = new List<AdviceArgument>();

        public abstract Kind Kind { get; }

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            if (Target.HasFlag(Target.Method) && target is MethodDefinition method && method.IsNormalMethod())
                return IsApplicableForModifier(method);

            if (Target.HasFlag(Target.Constructor) && target is MethodDefinition ctor && ctor.IsConstructor)
                return IsApplicableForModifier(ctor);

            if (Target.HasFlag(Target.Setter) && target is PropertyDefinition setter && setter.SetMethod != null)
                return IsApplicableForModifier(setter.SetMethod);

            if (Target.HasFlag(Target.Getter) && target is PropertyDefinition getter && getter.GetMethod != null)
                return IsApplicableForModifier(getter.GetMethod);

            if (Target.HasFlag(Target.EventAdd) && target is EventDefinition add && add.AddMethod != null)
                return IsApplicableForModifier(add.AddMethod);

            if (Target.HasFlag(Target.EventRemove) && target is EventDefinition remove && remove.RemoveMethod != null)
                return IsApplicableForModifier(remove.RemoveMethod);

            return false;
        }


        protected bool IsApplicableForModifier(MethodDefinition target)
        {
            if (
                (WithAccess.HasFlag(AccessModifier.Instance) && !target.IsStatic)
                || (WithAccess.HasFlag(AccessModifier.Static) && target.IsStatic)
                )
            {
                if (
                    (WithAccess.HasFlag(AccessModifier.Private) && target.IsPrivate)
                    || (WithAccess.HasFlag(AccessModifier.Public) && target.IsPublic)
                    || (WithAccess.HasFlag(AccessModifier.Protected) && target.IsFamily)
                    || (WithAccess.HasFlag(AccessModifier.ProtectedInternal) && target.IsFamilyOrAssembly)
                    || (WithAccess.HasFlag(AccessModifier.ProtectedPrivate) && target.IsFamilyAndAssembly)
                    || (WithAccess.HasFlag(AccessModifier.Internal) && target.IsAssembly)
                    )
                    return true;
            }

            return false;
        }

        protected override bool IsEqualTo(Effect effect)
        {
            var other = effect as AdviceEffectBase;

            if (other == null)
                return false;

            return other.Target == Target && other.Kind == Kind && other.Method == Method;
        }

        public override bool Validate(AspectDefinition aspect, ILogger log)
        {
            if (Method.IsStatic)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} must not be static.", aspect.Host));
                return false;
            }

            if (!Method.IsPublic)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} must be public.", aspect.Host));
                return false;
            }

            if (Method.HasGenericParameters)
            {
                log.LogError(CompilationMessage.From($"Advice {Method.FullName} must not be generic.", aspect.Host));
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Kind.ToString()}{Target.ToString()}";
        }
    }
}