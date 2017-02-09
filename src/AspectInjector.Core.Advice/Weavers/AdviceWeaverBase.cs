using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using AspectInjector.Core.Services;
using Mono.Cecil;
using System;
using System.Linq;
using static AspectInjector.Broker.Advice;
using static AspectInjector.Broker.Advice.Argument;

namespace AspectInjector.Core.Advice.Weavers
{
    internal abstract class AdviceWeaverBase<TEffect> : EffectWeaverBase<IMemberDefinition, TEffect>
        where TEffect : AdviceEffectBase
    {
        public AdviceWeaverBase(ILogger logger) : base(logger)
        {
        }

        protected override bool CanWeave(Injection injection)
        {
            var result = base.CanWeave(injection) &&
                (injection.Target is EventDefinition || injection.Target is PropertyDefinition || injection.Target is MethodDefinition);

            if (result && injection.Target is MethodDefinition && injection.Effect is AfterAdviceEffect)
            {
                var md = (MethodDefinition)injection.Target;
                if (md.IsAsync() || md.IsIterator())
                    result = false;
            }

            return result;
        }

        protected override void Weave(ResolvedInjection injection)
        {
            if (injection.Target is EventDefinition)
            {
                WeaveTargetEvent((EventDefinition)injection.Target, injection);
                return;
            }

            if (injection.Target is PropertyDefinition)
            {
                WeaveTargetProperty((PropertyDefinition)injection.Target, injection);
                return;
            }

            if (injection.Target is MethodDefinition)
            {
                WeaveTargetMethod((MethodDefinition)injection.Target, injection);
                return;
            }

            _log.LogError(CompilationMessage.From($"Unsupported target {injection.Target.GetType().Name}", injection.Target));
        }

        protected abstract void WeaveMethod(MethodDefinition method, ResolvedInjection injection);

        protected virtual void WeaveTargetProperty(PropertyDefinition target, ResolvedInjection injection)
        {
            if (target.SetMethod != null && injection.Effect.Target.HasFlag(Target.Setter))
                WeaveMethod(target.SetMethod, injection);

            if (target.GetMethod != null && injection.Effect.Target.HasFlag(Target.Getter))
                WeaveMethod(target.GetMethod, injection);
        }

        protected virtual void WeaveTargetMethod(MethodDefinition target, ResolvedInjection injection)
        {
            if (target.IsConstructor && injection.Effect.Target.HasFlag(Target.Constructor))
                WeaveMethod(target, injection);

            if (target.IsNormalMethod() && injection.Effect.Target.HasFlag(Target.Method))
                WeaveMethod(target, injection);
        }

        protected virtual void WeaveTargetEvent(EventDefinition target, ResolvedInjection injection)
        {
            if (target.AddMethod != null && injection.Effect.Target.HasFlag(Target.EventAdd))
                WeaveMethod(target.AddMethod, injection);

            if (target.RemoveMethod != null && injection.Effect.Target.HasFlag(Target.EventRemove))
                WeaveMethod(target.RemoveMethod, injection);
        }

        protected void LoadAdviceArgs(PointCut pc, MethodDefinition method, ResolvedInjection injection)
        {
            foreach (var arg in injection.Effect.Arguments.OrderBy(a => a.Parameter.Index))
            {
                switch (arg.Source)
                {
                    case Source.Arguments: LoadArgumentsArgument(pc, method, arg, injection); break;
                    case Source.Attributes: LoadAttributesArgument(pc, method, arg, injection); break;
                    case Source.Instance: LoadInstanceArgument(pc, method, arg, injection); break;
                    case Source.Method: LoadMethodArgument(pc, method, arg, injection); break;
                    case Source.Name: LoadNameArgument(pc, method, arg, injection); break;
                    case Source.ReturnType: LoadReturnTypeArgument(pc, method, arg, injection); break;
                    case Source.ReturnValue: LoadReturnValueArgument(pc, method, arg, injection); break;
                    case Source.Target: LoadTargetArgument(pc, method, arg, injection); break;
                    case Source.Type: LoadTypeArgument(pc, method, arg, injection); break;
                    default: _log.LogError(CompilationMessage.From($"Unknown argument source {arg.Source.ToString()}", method)); break;
                }
            }
        }

        protected virtual void LoadTypeArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            pc.TypeOf(target.DeclaringType);
        }

        protected virtual void LoadTargetArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            _log.LogWarning(CompilationMessage.From($"Advice {injection.Effect.Type.ToString()} does not support {parameter.Source.ToString()} argument and will always return null", injection.Effect.Method));
            pc.Null();
        }

        protected virtual void LoadReturnValueArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            _log.LogWarning(CompilationMessage.From($"Advice {injection.Effect.Type.ToString()} does not support {parameter.Source.ToString()} argument and will always return null", injection.Effect.Method));
            pc.Null();
        }

        protected virtual void LoadReturnTypeArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            pc.TypeOf(target.ReturnType);
        }

        protected virtual void LoadMethodArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            pc.MethodOf(target).Cast(injection.TypeSystem.MethodBase);
        }

        protected virtual void LoadInstanceArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            if (target.IsStatic)
                pc.Null();
            else
                pc.This();
        }

        protected virtual void LoadAttributesArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            pc.Null();
        }

        protected virtual void LoadArgumentsArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            var elements = target.Parameters.Select<ParameterDefinition, Action<PointCut>>(p => il =>
                il.Load(p).ByVal(p.ParameterType)
            ).ToArray();

            pc.CreateArray<object>(elements);
        }

        protected virtual void LoadNameArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        {
            pc.Value(injection.Target.Name);
        }
    }
}