using AspectInjector.Broker;
using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
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

        protected Cut LoadAdviceArgs(Cut cut)
        {
            foreach (var arg in _effect.Arguments.OrderBy(a => a.Parameter.Index))
            {
                switch (arg.Source)
                {
                    case Source.Arguments: cut = LoadArgumentsArgument(cut, arg); break;
                    case Source.Triggers: cut = LoadInjectionsArgument(cut, arg); break;
                    case Source.Instance: cut = LoadInstanceArgument(cut, arg); break;
                    case Source.Metadata: cut = LoadMethodArgument(cut, arg); break;
                    case Source.Name: cut = LoadNameArgument(cut, arg); break;
                    case Source.ReturnType: cut = LoadReturnTypeArgument(cut, arg); break;
                    case Source.ReturnValue: cut = LoadReturnValueArgument(cut, arg); break;
                    case Source.Target: cut = LoadTargetArgument(cut, arg); break;
                    case Source.Type: cut = LoadTypeArgument(cut, arg); break;
                    default: _log.Log(GeneralRules.UnexpectedCompilerBehaviour, _method, $"Unexpected argument source '{arg.Source.ToString()}'"); break;
                }
            }
            return cut;
        }

        protected virtual Cut LoadTypeArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_type);
        }

        protected virtual Cut LoadTargetArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.Null();
        }

        protected virtual Cut LoadReturnValueArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.Null();
        }

        protected virtual Cut LoadReturnTypeArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.TypeOf(_method.ReturnType);
        }

        protected virtual Cut LoadMethodArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.MethodOf(_method).Cast(StandardTypes.Object, WellKnownTypes.MethodBase);
        }

        protected virtual Cut LoadInstanceArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.ThisOrNull();
        }

        protected virtual Cut LoadInjectionsArgument(Cut pc, AdviceArgument parameter)
        {
            var elements = _injection.Triggers.Select<CustomAttribute, PointCut>(ca => il =>
            {
                var ctor = ca.Constructor.Resolve();

                il = il.Call(ctor, new PointCut(ilc =>
                {
                    foreach (var caa in ca.ConstructorArguments)
                        ilc = ilc.Value(caa);
                    return ilc;
                }));

                if (ca.Properties.Any() || ca.Fields.Any())
                {
                    var catype = ca.AttributeType.Resolve();

                    var attrvar = new VariableDefinition(il.Import(ca.AttributeType));
                    _method.Body.Variables.Add(attrvar);
                    _method.Body.InitLocals = true;

                    il = il.Store(attrvar);

                    foreach (var namedArg in ca.Properties)
                    {
                        var prop = catype.Properties.First(p => p.Name == namedArg.Name).SetMethod;

                        il = il
                        .Load(attrvar)
                        .Call(prop, ilp => ilp.Value(namedArg.Argument));
                    }

                    foreach (var namedArg in ca.Fields)
                    {
                        var field = catype.Fields.First(p => p.Name == namedArg.Name);

                        il = il
                        .Load(attrvar)
                        .Store(field, ilf => ilf.Value(namedArg.Argument));
                    }
                    il = il.Load(attrvar);
                }

                return il;
            }
            ).ToArray();

            return pc.CreateArray(StandardTypes.Attribute, elements);
        }

        protected virtual Cut LoadArgumentsArgument(Cut pc, AdviceArgument parameter)
        {
            var elements = _method.Parameters.Select<ParameterDefinition, PointCut>(p => il =>
                il.Load(p).Cast(p.ParameterType, StandardTypes.Object)
            ).ToArray();

            return pc.CreateArray(StandardTypes.Object, elements);
        }

        protected virtual Cut LoadNameArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.Value(_injection.Target.Name);
        }
    }
}