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
        private readonly string _wrapperNamePrefix;
        private readonly string _unWrapperName;
        private readonly string _movedOriginalName;

        public AdviceAroundProcess(ILogger log, AspectDefinition aspect, MethodDefinition target, AroundAdviceEffect effect) : base(log, target, effect, aspect)
        {
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
            var newUnwrapper = DuplicateMethodDefinition(unwrapper);
            newUnwrapper.IsPrivate = true;

            unwrapper.Name = name;

            MoveBody(unwrapper, newUnwrapper);

            return unwrapper;
        }

        private MethodDefinition GetOrCreateUnwrapper()
        {
            var unwrapper = _target.DeclaringType.Methods.FirstOrDefault(m => m.Name == _unWrapperName);
            if (unwrapper != null)
                return unwrapper;

            unwrapper = DuplicateMethodDefinition(_target);
            unwrapper.Name = _unWrapperName;
            unwrapper.ReturnType = _ts.Object;
            unwrapper.IsPrivate = true;
            unwrapper.Parameters.Clear();
            var argsParam = new ParameterDefinition(_ts.ObjectArray);
            unwrapper.Parameters.Add(argsParam);
            unwrapper.Body.InitLocals = true;

            var original = WrapEntryPoint(unwrapper);

            unwrapper.GetEditor().Instead(
                il =>
                {
                    il = il.ThisOrStatic().Call(original, c =>
                    {
                        for (int i = 0; i < original.Parameters.Count; i++)
                        {
                            var p = original.Parameters[i];

                            c = c.Load(argsParam);

                            if (p.ParameterType.IsByReference)
                            {
                                var elementType = ((ByReferenceType)p.ParameterType).ElementType;

                                var tempVar = new VariableDefinition($"{Constants.Prefix}p_{p.Name}", elementType);
                                unwrapper.Body.Variables.Add(tempVar);

                                c.Store(tempVar, v => v.GetByIndex(i).Cast(elementType));
                                c.LoadRef(tempVar);

                                //if (p.ParameterType.IsValueType || p.ParameterType.IsGenericParameter)
                                //{
                                //    c.Value((object)null);
                                //}
                                //else
                                //{
                                //    c = c.GetAddrByIndex(i, _ts.Object);
                                //    c = c.Cast(elementType);
                                //}
                            }
                            else
                            {
                                c = c.GetByIndex(i);

                                if (p.ParameterType.IsGenericParameter || !p.ParameterType.IsTypeOf(_ts.Object))
                                    c = c.Cast(p.ParameterType);
                            }

                            //if (p.ParameterType.IsValueType || p.ParameterType.IsGenericParameter)
                            //{
                            //    if (p.ParameterType.IsByReference)
                            //        c = c.GetAddrByIndex(i, _ts.Object);
                            //    else
                            //        c = c.GetByIndex(i).Cast(p.ParameterType);
                            //}
                            //else
                            //{
                            //    if (p.ParameterType.IsByReference)
                            //    {
                            //        c = c.GetAddrByIndex(i, _ts.Object);
                            //        c = c.Cast(((ByReferenceType)p.ParameterType).ElementType);
                            //    }
                            //    else
                            //    {
                            //        c = c.GetByIndex(i);

                            //        if (p.ParameterType.IsGenericParameter || !p.ParameterType.IsTypeOf(_ts.Object))
                            //            c = c.Cast(p.ParameterType);
                            //    }
                            //}

                            //if (p.ParameterType.IsByReference)
                            //c = c.ByRef(p.ParameterType);
                            //else
                        }
                    });

                    if (original.ReturnType.IsTypeOf(_ts.Void))
                        il = il.Value((object)null);
                    else if (original.ReturnType.IsValueType || original.ReturnType.IsGenericParameter)
                        il = il.ByVal(original.ReturnType);

                    il.Return();
                });

            return unwrapper;
        }

        private MethodDefinition WrapEntryPoint(MethodDefinition unwrapper)
        {
            var original = DuplicateMethodDefinition(_target);
            original.Name = _movedOriginalName;
            original.IsPrivate = true;

            var returnType = _target.ResolveGenericType(_target.ReturnType);

            MoveBody(_target, original);

            _target.GetEditor().Instead(
                e =>
                {
                    e = e.ThisOrStatic().Call(unwrapper, args => base.LoadArgumentsArgument(args, null));

                    //if (_target.ReturnType.IsGenericParameter)
                    //{
                    //    e = e.Cast(returnType);
                    //}
                    //else
                    if (returnType.IsTypeOf(_ts.Void))
                        e = e.Pop();
                    //else if (_target.ReturnType.IsValueType)
                    //    e = e.ByRef(_target.ReturnType);
                    else if (!returnType.IsTypeOf(_ts.Object))
                        e = e.Cast(returnType);
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

            //erase old body
            from.Body.Instructions.Clear();
            from.Body = new MethodBody(from);
        }

        private MethodDefinition DuplicateMethodDefinition(MethodDefinition origin)
        {
            var method = new MethodDefinition(origin.Name,
               origin.Attributes,
               origin.ReturnType);

            foreach (var gparam in origin.GenericParameters)
                method.GenericParameters.Add(new GenericParameter(gparam.Name, method));

            if (origin.ReturnType.IsGenericParameter && ((GenericParameter)origin.ReturnType).Owner == origin)
                method.ReturnType = method.GenericParameters[origin.GenericParameters.IndexOf((GenericParameter)origin.ReturnType)];

            if (origin.IsSpecialName)
                method.IsSpecialName = true;

            foreach (var parameter in origin.Parameters)
            {
                var paramType = parameter.ParameterType;
                if (paramType.IsGenericParameter && ((GenericParameter)paramType).Owner == origin)
                    paramType = method.GenericParameters[origin.GenericParameters.IndexOf((GenericParameter)paramType)];

                method.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, paramType));
            }

            origin.DeclaringType.Methods.Add(method);

            return method;
        }

        private static string GetAroundMethodPrefix(MethodDefinition target)
        {
            return $"{Constants.Prefix}around_{target.Name}_{target.MetadataToken.ToUInt32()}_";
        }
    }
}