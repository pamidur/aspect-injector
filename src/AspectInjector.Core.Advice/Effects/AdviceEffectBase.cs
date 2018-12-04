using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.Core.Advice.Effects
{
    internal abstract class AdviceEffectBase : Effect
    {
        public Target Target { get; set; }

        public MethodDefinition Method { get; set; }
        public List<AdviceArgument> Arguments { get; set; } = new List<AdviceArgument>();

        public abstract Kind Kind { get; }

        public override bool IsApplicableFor(IMemberDefinition target)
        {
            if ((Target & Target.Method) != 0 && target is MethodDefinition method && method.IsNormalMethod())
                return IsApplicableForModifier(method);

            if ((Target & Target.Constructor) != 0 && target is MethodDefinition ctor && ctor.IsConstructor)
                return IsApplicableForModifier(ctor);

            if ((Target & Target.Setter) != 0 && target is PropertyDefinition setter && setter.SetMethod != null)
                return IsApplicableForModifier(setter.SetMethod);

            if ((Target & Target.Getter) != 0 && target is PropertyDefinition getter && getter.GetMethod != null)
                return IsApplicableForModifier(getter.GetMethod);

            if ((Target & Target.EventAdd) != 0 && target is EventDefinition add && add.AddMethod != null)
                return IsApplicableForModifier(add.AddMethod);

            if ((Target & Target.EventRemove) != 0 && target is EventDefinition remove && remove.RemoveMethod != null)
                return IsApplicableForModifier(remove.RemoveMethod);

            return false;
        }


        protected bool IsApplicableForModifier(MethodDefinition target)
        {
            if (
                ((Target & Target.Instance) != 0 && !target.IsStatic)
                || ((Target & Target.Static) != 0 && target.IsStatic)
                )
            {
                if (
                    ((Target & Target.Private) != 0 && target.IsPrivate)
                    || ((Target & Target.Public) != 0 && target.IsPublic)
                    || ((Target & Target.Protected) != 0 && target.IsFamily)
                    || ((Target & Target.ProtectedInternal) != 0 && target.IsFamilyOrAssembly)
                    || ((Target & Target.ProtectedPrivate) != 0 && target.IsFamilyAndAssembly)
                    || ((Target & Target.Internal) != 0 && target.IsAssembly)
                    )
                    return true;
            }

            return false;
        }

        protected override bool IsEqualTo(Effect effect)
        {
            if (!(effect is AdviceEffectBase other))
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