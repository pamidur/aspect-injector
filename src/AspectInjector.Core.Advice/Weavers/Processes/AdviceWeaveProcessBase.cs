using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal abstract class AdviceWeaveProcessBase<TEffect>
        where TEffect : AdviceEffectBase
    {
        protected readonly MethodDefinition _method;
        protected readonly TypeDefinition _type;
        protected readonly ModuleDefinition _module;
        protected readonly TEffect _effect;
        protected readonly InjectionDefinition _injection;
        protected readonly ILogger _log;
        protected readonly AspectDefinition _aspect;

        protected AdviceWeaveProcessBase(ILogger log, MethodDefinition target, InjectionDefinition injection)
        {
            _log = log;
            _method = target;
            _type = target.DeclaringType;
            _module = target.Module;
            _effect = (TEffect)injection.Effect;
            _injection = injection;
            _aspect = injection.Source;
        }

        public abstract void Execute();

        protected Cut LoadAdviceArgs(in Cut cut)
        {
            var cur_cut = cut;
            foreach (var arg in _effect.Arguments.OrderBy(a => a.Parameter.Index))
            {
                switch (arg.Source)
                {
                    case Source.Arguments: cur_cut = LoadArgumentsArgument(cur_cut, arg); break;
                    case Source.Triggers: cur_cut = LoadInjectionsArgument(cur_cut, arg); break;
                    case Source.Instance: cur_cut = LoadInstanceArgument(cur_cut, arg); break;
                    case Source.Metadata: cur_cut = LoadMethodArgument(cur_cut, arg); break;
                    case Source.Name: cur_cut = LoadNameArgument(cur_cut, arg); break;
                    case Source.ReturnType: cur_cut = LoadReturnTypeArgument(cur_cut, arg); break;
                    case Source.ReturnValue: cur_cut = LoadReturnValueArgument(cur_cut, arg); break;
                    case Source.Target: cur_cut = LoadTargetArgument(cur_cut, arg); break;
                    case Source.Type: cur_cut = LoadTypeArgument(cur_cut, arg); break;
                    default: _log.Log(GeneralRules.UnexpectedCompilerBehaviour, _method, $"Unexpected argument source '{arg.Source.ToString()}'"); break;
                }
            }
            return cur_cut;
        }

        protected virtual Cut LoadTypeArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_type);
        }

        protected virtual Cut LoadTargetArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.Null();
        }

        protected virtual Cut LoadReturnValueArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.Null();
        }

        protected virtual Cut LoadReturnTypeArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_method.ReturnType);
        }

        protected virtual Cut LoadMethodArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.MethodOf(_method).Cast(pc.TypeSystem.Object, pc.Import(StandardType.MethodBase));
        }

        protected virtual Cut LoadInstanceArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.ThisOrNull();
        }

        protected virtual Cut LoadInjectionsArgument(in Cut pc, AdviceArgument parameter)
        {
            var elements = _injection.Triggers.Select<CustomAttribute, PointCut>(ca => (in Cut cut) =>
            {
                var ctor = ca.Constructor.Resolve();

                var il = cut.Call(ctor, new PointCut((in Cut ilc_cut) =>
                {
                    var ilc = ilc_cut;
                    foreach (var caa in ca.ConstructorArguments)
                        ilc = ilc.Value(caa);
                    return ilc;
                }));

                if (ca.Properties.Any() || ca.Fields.Any())
                {
                    var catype = ca.AttributeType.Resolve();

                    foreach (var namedArg in ca.Properties)
                    {
                        var prop = catype.FindProperty(namedArg.Name).SetMethod;

                        il = il
                        .Dup()
                        .Call(prop, (in Cut ilp) => ilp.Value(namedArg.Argument));
                    }

                    foreach (var namedArg in ca.Fields)
                    {
                        var field = catype.FindField(namedArg.Name);

                        il = il
                        .Dup()
                        .Store(field, (in Cut ilf) => ilf.Value(namedArg.Argument));
                    }
                }

                return il;
            }
            ).ToArray();

            return pc.CreateArray(pc.Import(StandardType.Attribute), elements);
        }

        protected virtual Cut LoadArgumentsArgument(in Cut pc, AdviceArgument parameter)
        {
            var elements = _method.Parameters.Select<ParameterDefinition, PointCut>(p => (in Cut il) =>
                il.Load(p).Cast(p.ParameterType, il.TypeSystem.Object)
            ).ToArray();

            return pc.CreateArray(pc.TypeSystem.Object, elements);
        }

        protected virtual Cut LoadNameArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.Value(_injection.Target.Name);
        }
    }
}