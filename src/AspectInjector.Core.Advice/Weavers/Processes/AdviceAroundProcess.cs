using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceAroundProcess : AdviceWeaveProcessBase<AroundAdviceEffect>
    {
        private readonly AspectDefinition _aspect;
        private readonly string _wrapperNamePrefix;
        private readonly string _unWrapperName;
        private readonly string _movedOriginalName;

        public AdviceAroundProcess(ILogger log, AspectDefinition aspect, MethodDefinition target, AroundAdviceEffect effect) : base(log, target, effect)
        {
            _aspect = aspect;
            _wrapperNamePrefix = $"{GetAroundMethodPrefix(_target)}w_";
            _unWrapperName = $"{GetAroundMethodPrefix(_target)}u";
            _movedOriginalName = $"{GetAroundMethodPrefix(_target)}o";
        }

        public override void Execute()
        {
            var wrapper = GetNextWrapper();

            wrapper.GetEditor().Instead(
                e => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
                .Return()
            );
        }

        //protected override void LoadTargetArgument(PointCut pc, MethodDefinition target, AdviceArgument parameter, ResolvedInjection injection)
        //{
        //    var unwapper = GetOrCreateUnwrapper(target, injection);

        //    var targetFuncType = TypeSystem.MakeGenericInstanceType(
        //        TypeSystem.FuncGeneric2,
        //        TypeSystem.MakeArrayType(TypeSystem.Object),
        //        TypeSystem.Object);

        //    var targetFuncCtor = ModuleContext.ModuleDefinition.Import(targetFuncType.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic))
        //       .MakeGeneric(targetFuncType);

        //    if (!Processor.Body.Method.IsStatic)
        //        LoadSelfOntoStack();
        //    else
        //        Processor.InsertBefore(InjectionPoint, Processor.Create(OpCodes.Ldnull));

        //    InsertBefore(CreateInstruction(OpCodes.Ldftn, _nextWrapper));
        //    InsertBefore(CreateInstruction(OpCodes.Newobj, (MethodReference)CreateMemberReference(targetFuncCtor)));

        //}

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.Value((object)null);
        }

        public MethodDefinition GetNextWrapper()
        {
            var wrapper = _target.DeclaringType.Methods.Where(m => m.Name.StartsWith(_wrapperNamePrefix))
                .Select(m => new { m, i = ushort.Parse(m.Name.Substring(_wrapperNamePrefix.Length)) }).OrderByDescending(g => g.i).FirstOrDefault();

            return ReplaceUnwrapper($"{_wrapperNamePrefix}{(wrapper == null ? 0 : wrapper.i + 1)}");
        }

        private MethodDefinition ReplaceUnwrapper(string name)
        {
            var unwrapper = GetOrCreateUnwrapper();
            unwrapper.Name = name;

            var newUnwrapper = new MethodDefinition(_unWrapperName,
                _target.Attributes,
                _ts.Object);

            newUnwrapper.IsPrivate = true;

            MoveBody(unwrapper, newUnwrapper);

            _target.DeclaringType.Methods.Add(newUnwrapper);

            return unwrapper;
        }

        private MethodDefinition GetOrCreateUnwrapper()
        {
            var unwrapper = _target.DeclaringType.Methods.FirstOrDefault(m => m.Name == _unWrapperName);
            if (unwrapper != null)
                return unwrapper;

            unwrapper = new MethodDefinition(_unWrapperName,
                _target.Attributes,
                _ts.Object);

            unwrapper.IsPrivate = true;

            unwrapper.NoInlining = false;

            foreach (var gp in _target.GenericParameters)
                unwrapper.GenericParameters.Add(new GenericParameter(gp.Name, unwrapper));

            var argsParam = new ParameterDefinition(_ts.ObjectArray);
            unwrapper.Parameters.Add(argsParam);

            _target.DeclaringType.Methods.Add(unwrapper);

            var original = WrapEntryPoint(unwrapper);

            var callingMethod = unwrapper.ParametrizeGenericChild(original);

            unwrapper.GetEditor().Instead(
                e =>
                {
                    e = e.ThisOrNothing().Call(callingMethod, c =>
                    {
                        for (int i = 0; i < original.Parameters.Count; i++)
                        {
                            var p = original.Parameters[i];

                            c = c.Load(argsParam);

                            if (p.ParameterType.IsByReference)
                                c = c.GetAddrByIndex(i, _ts.Object);
                            else
                            {
                                c = c.GetByIndex(i);
                                if (p.ParameterType.IsGenericParameter || !p.ParameterType.IsTypeOf(_ts.Object))
                                    c = c.Cast(p.ParameterType);
                            }

                            //if (p.ParameterType.IsByReference)
                            //c = c.ByRef(p.ParameterType);
                            //else
                        }
                    });

                    if (original.ReturnType.IsTypeOf(_ts.Void))
                        e = e.Value((object)null);
                    else if (original.ReturnType.IsValueType)
                        e = e.ByVal(original.ReturnType);

                    e.Return();
                });

            return unwrapper;
        }

        private MethodDefinition WrapEntryPoint(MethodDefinition unwrapper)
        {
            var original = new MethodDefinition(_movedOriginalName,
                _target.Attributes,
                _target.ReturnType);

            original.IsPrivate = true;

            MoveBody(_target, original);

            _target.DeclaringType.Methods.Add(original);

            var callingMethod = _target.ParametrizeGenericChild(unwrapper);

            _target.GetEditor().Instead(
                e =>
                {
                    e = e.ThisOrNothing().Call(callingMethod, args => base.LoadArgumentsArgument(args, null));

                    if (_target.ReturnType.IsTypeOf(_ts.Void))
                        e = e.Pop();
                    //else if (_target.ReturnType.IsValueType)
                    //    e = e.ByRef(_target.ReturnType);
                    else if (!_target.ReturnType.IsTypeOf(_ts.Object))
                        e = e.Cast(_target.ReturnType);
                    e.Return();
                });

            return original;
        }

        private void MoveBody(MethodDefinition from, MethodDefinition to)
        {
            foreach (var inst in from.Body.Instructions)
                to.Body.Instructions.Add(inst);

            foreach (var var in from.Body.Variables)
                to.Body.Variables.Add(new VariableDefinition(var.Name, _ts.Import(var.VariableType)));

            if (to.Body.HasVariables)
                to.Body.InitLocals = true;

            foreach (var handler in from.Body.ExceptionHandlers)
                to.Body.ExceptionHandlers.Add(handler);

            foreach (var parameter in from.Parameters)
                to.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, _ts.Import(parameter.ParameterType)));

            foreach (var gparam in from.GenericParameters)
                to.GenericParameters.Add(new GenericParameter(gparam.Name, to));

            //erase old body
            from.Body.Instructions.Clear();
            from.Body = new MethodBody(from);
        }

        private static string GetAroundMethodPrefix(MethodDefinition target)
        {
            return $"{Constants.Prefix}around_{target.Name}_{target.MetadataToken.ToUInt32()}_";
        }
    }
}