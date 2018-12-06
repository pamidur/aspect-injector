using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Fluent.Models;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AdviceWeaveProcessBase<TEffect>
        where TEffect : AdviceEffectBase
    {
        protected readonly MethodDefinition _target;
        protected readonly TEffect _effect;
        protected readonly InjectionDefinition _injection;
        protected readonly ExtendedTypeSystem _ts;
        protected readonly ILogger _log;
        protected readonly AspectDefinition _aspect;

        public AdviceWeaveProcessBase(ILogger log, MethodDefinition target, InjectionDefinition injection)
        {
            _log = log;
            _target = target;
            _effect = (TEffect)injection.Effect;
            _injection = injection;
            _aspect = injection.Source;

            _ts = target.Module.GetTypeSystem();
        }

        public abstract void Execute();

        protected void LoadAdviceArgs(PointCut pc)
        {
            foreach (var arg in _effect.Arguments.OrderBy(a => a.Parameter.Index))
            {
                switch (arg.Source)
                {
                    case Source.Arguments: LoadArgumentsArgument(pc, arg); break;
                    case Source.Injections: LoadInjectionsArgument(pc, arg); break;
                    case Source.Instance: LoadInstanceArgument(pc, arg); break;
                    case Source.Method: LoadMethodArgument(pc, arg); break;
                    case Source.Name: LoadNameArgument(pc, arg); break;
                    case Source.ReturnType: LoadReturnTypeArgument(pc, arg); break;
                    case Source.ReturnValue: LoadReturnValueArgument(pc, arg); break;
                    case Source.Target: LoadTargetArgument(pc, arg); break;
                    case Source.Type: LoadTypeArgument(pc, arg); break;
                    default: _log.Log(GeneralRules.UnknownCompilationOption,_target,$"Unknown argument source '{arg.Source.ToString()}'"); break;
                }
            }
        }

        protected virtual void LoadTypeArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.TypeOf(_target.DeclaringType);
        }

        protected virtual void LoadTargetArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.Null();
        }

        protected virtual void LoadReturnValueArgument(PointCut pc, AdviceArgument parameter)
        {
           pc.Null();
        }

        protected virtual void LoadReturnTypeArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.TypeOf(_target.ReturnType);
        }

        protected virtual void LoadMethodArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.MethodOf(_target).Cast(_ts.MethodBase);
        }

        protected virtual void LoadInstanceArgument(PointCut pc, AdviceArgument parameter)
        {
            if (_target.IsStatic)
                pc.Null();
            else
                pc.This();
        }

        protected virtual void LoadInjectionsArgument(PointCut pc, AdviceArgument parameter)
        {
            var elements = _injection.Triggers.Select<CustomAttribute, Action<PointCut>>(ca => il =>
            {
                var ctor = ca.Constructor.Resolve();
                void ctorParams(PointCut ilc) => ca.ConstructorArguments.Select(caa => ilc.Value(caa)).ToArray();
                il.Call(ctor, ctorParams);

                if (ca.Properties.Any() || ca.Fields.Any())
                {
                    var catype = ca.AttributeType.Resolve();

                    var attrvar = new VariableDefinition(_ts.Import(ca.AttributeType));
                    _target.Body.Variables.Add(attrvar);
                    _target.Body.InitLocals = true;

                    il.Store(attrvar);

                    foreach (var namedArg in ca.Properties)
                    {
                        var prop = catype.Properties.First(p => p.Name == namedArg.Name).SetMethod;

                        il.Load(attrvar);
                        il.Call(prop, ilp => ilp.Value(namedArg.Argument));
                    }

                    foreach (var namedArg in ca.Fields)
                    {
                        var field = catype.Fields.First(p => p.Name == namedArg.Name);

                        il.Load(attrvar);
                        il.Store(field, ilf => ilf.Value(namedArg.Argument));
                    }
                    il.Load(attrvar);
                }
            }
            ).ToArray();

            pc.CreateArray(_ts.Attribute, elements);
        }

        protected virtual void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            var elements = _target.Parameters.Select<ParameterDefinition, Action<PointCut>>(p => il =>
                il.Load(p).ByVal(p.ParameterType)
            ).ToArray();

            pc.CreateArray(_ts.Object, elements);
        }

        protected virtual void LoadNameArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.Value(_injection.Target.Name);
        }
    }
}