using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using Mono.Cecil;
using System.Collections.Generic;
using System.Linq;

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
            ValidateSupportedArguments(aspect, log);

            var result = true;

            if (Method.IsStatic)
            {
                log.Log(EffectRules.AdviceMustHaveValidSingnature, Method, Method.Name, EffectRules.Literals.IsStatic);
                result = false;
            }

            if (!Method.IsPublic)
            {
                log.Log(EffectRules.AdviceMustHaveValidSingnature, Method, Method.Name, EffectRules.Literals.IsNotPublic);
                result = false;
            }

            if (Method.HasGenericParameters)
            {
                log.Log(EffectRules.AdviceMustHaveValidSingnature, Method, Method.Name, EffectRules.Literals.IsGeneric);
                result = false;
            }

            return result;
        }

        protected virtual void ValidateSupportedArguments(AspectDefinition aspectDefinition,ILogger log)
        {
            var wrongArgs = Arguments.Where(a => a.Source == Source.Target || a.Source == Source.ReturnValue).ToArray();
            LogWrongArgs(wrongArgs, aspectDefinition, log);
        }

        protected void LogWrongArgs(AdviceArgument[] wrongArgs, AspectDefinition aspectDefinition, ILogger log)
        {
            foreach (var arg in wrongArgs)
                log.Log(EffectRules.ArgumentIsAlwaysNull, Method, arg.Parameter.Name, Kind.ToString());
        }

        public override string ToString()
        {
            return $"{Kind.ToString()}::{Target.ToString()}";
        }
    }
}