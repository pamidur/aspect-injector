using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Fluent;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceAroundProcess : AdviceWeaveProcessBase<AroundAdviceEffect>
    {
        private readonly string _wrapperNamePrefix;
        private readonly string _unWrapperName;
        private readonly string _movedOriginalName;
        private MethodDefinition _wrapper;

        public AdviceAroundProcess(ILogger log, MethodDefinition target, Injection injection) : base(log, target, injection)
        {
            _wrapperNamePrefix = $"{GetAroundMethodPrefix(_target)}w_";
            _unWrapperName = $"{GetAroundMethodPrefix(_target)}u";
            _movedOriginalName = $"{GetAroundMethodPrefix(_target)}o";
        }

        public override void Execute()
        {
            _wrapper = GetNextWrapper();

            _wrapper.GetEditor().Instead(
                e => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
                .Return()
            );
        }

        protected override void LoadTargetArgument(PointCut pc, AdviceArgument parameter)
        {
            var targetFuncType = _ts.MakeGenericInstanceType(
                _ts.FuncGeneric2,
                _ts.ObjectArray,
                _ts.Object);

            var targetFuncCtor = targetFuncType.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic).MakeHostInstanceGeneric(targetFuncType);
            var targetMethod = _wrapper.MakeCallReference(GetOrCreateUnwrapper().MakeHostInstanceGeneric(_target.DeclaringType));

            pc.ThisOrNull().Call(targetFuncCtor, args => args.Load(targetMethod));
        }

        protected override void LoadArgumentsArgument(PointCut pc, AdviceArgument parameter)
        {
            pc.Load(_wrapper.Parameters[0]);
        }

        public MethodDefinition GetNextWrapper()
        {
            var prevWrapper = _target.DeclaringType.Methods.Where(m => m.Name.StartsWith(_wrapperNamePrefix))
                .Select(m => new { m, i = ushort.Parse(m.Name.Substring(_wrapperNamePrefix.Length)) }).OrderByDescending(g => g.i).FirstOrDefault();

            var newWrapper = DuplicateMethodDefinition(GetOrCreateUnwrapper());
            newWrapper.IsPrivate = true;
            newWrapper.Name = $"{_wrapperNamePrefix}{(prevWrapper == null ? 0 : prevWrapper.i + 1)}";
            newWrapper.GetEditor().Mark<DebuggerHiddenAttribute>();

            RedirectPreviousWrapper(prevWrapper == null ? _target : prevWrapper.m, newWrapper);

            return newWrapper;
        }

        private void RedirectPreviousWrapper(MethodDefinition prev, MethodDefinition next)
        {
            var unwrapper = GetOrCreateUnwrapper();

            var instructions = prev.Body.Instructions.Where(i => i.Operand is MethodReference && ((MethodReference)i.Operand).Resolve() == unwrapper).ToList();

            foreach (var inst in instructions)
                inst.Operand = prev.MakeCallReference(next.MakeHostInstanceGeneric(_target.DeclaringType));
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
            unwrapper.GetEditor().Mark<DebuggerHiddenAttribute>();

            var original = WrapEntryPoint(unwrapper);

            unwrapper.GetEditor().Instead(
                il =>
                {
                    var refList = new List<Tuple<int, VariableDefinition>>();

                    il = il.ThisOrStatic().Call(original.MakeHostInstanceGeneric(_target.DeclaringType), c =>
                    {
                        for (int i = 0; i < original.Parameters.Count; i++)
                        {
                            var p = original.Parameters[i];

                            if (p.ParameterType.IsByReference)
                            {
                                var elementType = ((ByReferenceType)p.ParameterType).ElementType;

                                var tempVar = new VariableDefinition(elementType);
                                refList.Add(new Tuple<int, VariableDefinition>(i, tempVar));
                                unwrapper.Body.Variables.Add(tempVar);

                                c.Store(tempVar, v => v.Load(argsParam).GetByIndex(i).Cast(elementType));
                                c.LoadRef(tempVar);
                            }
                            else
                            {
                                c = c.Load(argsParam).GetByIndex(i);

                                if (p.ParameterType.IsGenericParameter || p.ParameterType.FullName != WellKnownTypes.Object)
                                    c = c.Cast(p.ParameterType);
                            }
                        }
                    });

                    foreach (var refPar in refList)
                        il = il.Load(argsParam).SetByIndex(_ts.Object, refPar.Item1,  val => val.Load(refPar.Item2).ByVal(refPar.Item2.VariableType));

                    if (original.ReturnType.FullName == WellKnownTypes.Void)
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

            var returnType = _target.ResolveIfGeneric(_target.ReturnType);

            MoveBody(_target, original);

            _target.GetEditor().Instead(
                e =>
                {
                    //var args = null;
                    var argsVar = new VariableDefinition(_ts.ObjectArray);
                    _target.Body.Variables.Add(argsVar);
                    _target.Body.InitLocals = true;

                    //args = new object[] { param1, param2 ...};
                    e.Store(argsVar, args => base.LoadArgumentsArgument(args, null));

                    // Unwrapper(args);
                    e.ThisOrStatic().Call(unwrapper.MakeHostInstanceGeneric(_target.DeclaringType), args => args.Load(argsVar));

                    // proxy ref and out params
                    for (int i = 0; i < _target.Parameters.Count; i++)
                    {
                        var p = _target.Parameters[i];
                        if (p.ParameterType.IsByReference)
                            e.StoreByRef(p, val => e.Load(argsVar).GetByIndex(i));
                    }

                    //drop if void, cast if not is object
                    if (returnType.FullName == WellKnownTypes.Void)
                        e = e.Pop();
                    else if (returnType.FullName != WellKnownTypes.Object)
                        e = e.Cast(returnType);

                    e.Return();
                });

            return original;
        }

        private void MoveBody(MethodDefinition from, MethodDefinition to)
        {
            foreach (var inst in from.Body.Instructions)            
                to.Body.Instructions.Add(inst);            

            if (from.DebugInformation.HasSequencePoints)
                foreach (var sp in from.DebugInformation.SequencePoints)
                    to.DebugInformation.SequencePoints.Add(sp);

            foreach (var var in from.Body.Variables)
                to.Body.Variables.Add(new VariableDefinition(_ts.Import(var.VariableType)));

            if (to.Body.HasVariables)
                to.Body.InitLocals = true;

            foreach (var handler in from.Body.ExceptionHandlers)
                to.Body.ExceptionHandlers.Add(handler);

            //erase old body
            from.DebugInformation.SequencePoints.Clear();
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