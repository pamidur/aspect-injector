using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Advice.Weavers.Processes
{
    internal class AdviceAroundProcess : AdviceWeaveProcessBase<AroundAdviceEffect>
    {
        private readonly string _wrapperNamePrefix;
        private readonly string _unWrapperName;
        private readonly string _movedOriginalName;
        private MethodDefinition _wrapper;

        public AdviceAroundProcess(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _wrapperNamePrefix = $"{GetAroundMethodPrefix(_target)}w_";
            _unWrapperName = $"{GetAroundMethodPrefix(_target)}u";
            _movedOriginalName = $"{GetAroundMethodPrefix(_target)}o";
        }

        public override void Execute()
        {
            _wrapper = GetNextWrapper();

            _wrapper.Body.Instead(
                e => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
                .Return()
            );
        }

        protected override Cut LoadTargetArgument(Cut pc, AdviceArgument parameter)
        {
            var targetMethod = _wrapper.MakeCallReference(GetOrCreateUnwrapper().MakeHostInstanceGeneric(_target.DeclaringType));
            return pc.ThisOrNull().Call(CreateFuncCtorRef(pc), args => args.Delegate(targetMethod));
        }
        
        private static MethodReference CreateFuncCtorRef(Cut cut)
        {
            var mr = new MethodReference(".ctor", cut.Import(StandardTypes.Void), cut.Import(WellKnownTypes.Func_ObjectArray_Object))
            {
                HasThis = true
            };
            mr.Parameters.Add(new ParameterDefinition(cut.Import(StandardTypes.Object)));
            mr.Parameters.Add(new ParameterDefinition(cut.Import(StandardTypes.IntPtr)));
            return mr;
        }

        protected override Cut LoadArgumentsArgument(Cut pc, AdviceArgument parameter)
        {
            return pc.Load(_wrapper.Parameters[0]);
        }

        public MethodDefinition GetNextWrapper()
        {
            var prevWrapper = _target.DeclaringType.Methods.Where(m => m.Name.StartsWith(_wrapperNamePrefix))
                .Select(m => new { m, i = ushort.Parse(m.Name.Substring(_wrapperNamePrefix.Length)) }).OrderByDescending(g => g.i).FirstOrDefault();

            var newWrapper = DuplicateMethodDefinition(GetOrCreateUnwrapper());
            newWrapper.IsPrivate = true;
            newWrapper.Name = $"{_wrapperNamePrefix}{(prevWrapper == null ? 0 : prevWrapper.i + 1)}";
            newWrapper.Mark(WellKnownTypes.DebuggerHiddenAttribute);

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
            unwrapper.ReturnType = _target.Module.ImportReference(StandardTypes.Object);
            unwrapper.IsPrivate = true;
            unwrapper.Parameters.Clear();
            var argsParam = new ParameterDefinition(_target.Module.ImportReference(StandardTypes.ObjectArray));
            unwrapper.Parameters.Add(argsParam);
            unwrapper.Body.InitLocals = true;
            unwrapper.Mark(WellKnownTypes.DebuggerHiddenAttribute);

            var original = WrapEntryPoint(unwrapper);

            unwrapper.Body.Instead(
                il =>
                {
                    var refList = new List<Tuple<int, VariableDefinition>>();

                    var targetMethod = original.MakeHostInstanceGeneric(_target.DeclaringType);

                    il = il.ThisOrStatic().Call(targetMethod, c =>
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

                                c = c
                                .Store(tempVar, v => v
                                    .Load(argsParam)
                                    .GetByIndex(StandardTypes.Object, i)
                                    .Cast(StandardTypes.Object, elementType))
                                .LoadRef(tempVar);
                            }
                            else
                            {
                                c = c
                                .Load(argsParam)
                                .GetByIndex(StandardTypes.Object, i)
                                .Cast(StandardTypes.Object, p.ParameterType);
                            }
                        }

                        return c;
                    });

                    foreach (var refPar in refList)
                        il = il.Load(argsParam).SetByIndex(StandardTypes.Object, refPar.Item1, val => val.Load(refPar.Item2).Cast(refPar.Item2.VariableType, StandardTypes.Object));

                    if (original.ReturnType.Match(StandardTypes.Void))
                        il = il.Value(null);
                    else
                        il = il.Cast(targetMethod.ReturnType, unwrapper.ReturnType);

                    return il.Return();
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

            _target.Body.Instead(
                e =>
                {
                    //var args = null;
                    var argsVar = new VariableDefinition(_target.Module.ImportReference(StandardTypes.ObjectArray));
                    _target.Body.Variables.Add(argsVar);
                    _target.Body.InitLocals = true;

                    //args = new object[] { param1, param2 ...};
                    e = e.Store(argsVar, args => base.LoadArgumentsArgument(args, null));

                    var targetMethod = unwrapper.MakeHostInstanceGeneric(_target.DeclaringType);

                    // Unwrapper(args);
                    e = e.ThisOrStatic().Call(targetMethod, args => args.Load(argsVar));

                    // proxy ref and out params
                    for (int i = 0; i < _target.Parameters.Count; i++)
                    {
                        var p = _target.Parameters[i];
                        if (p.ParameterType.IsByReference)
                            e = e.Store(p, val => val.Load(argsVar).GetByIndex(StandardTypes.Object, i).Cast(StandardTypes.Object, p.ParameterType));
                    }

                    //drop if void, cast if not is object
                    if (returnType.Match(StandardTypes.Void))
                        e = e.Pop();
                    else
                        e = e.Cast(targetMethod.ReturnType, returnType);

                    return e.Return();
                });

            return original;
        }

        private void MoveBody(MethodDefinition from, MethodDefinition to)
        {
            foreach (var inst in from.Body.Instructions)
                to.Body.Instructions.Add(inst);

            if (from.DebugInformation.HasSequencePoints)
            {
                to.DebugInformation.Scope = from.DebugInformation.Scope;
                foreach (var sp in from.DebugInformation.SequencePoints)
                    to.DebugInformation.SequencePoints.Add(sp);
            }

            foreach (var var in from.Body.Variables)
                to.Body.Variables.Add(new VariableDefinition(to.Module.ImportReference(var.VariableType)));

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

            origin.DeclaringType.Methods.Add(method);

            foreach (var gparam in origin.GenericParameters)
                method.GenericParameters.Add(gparam.Clone(method));

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

            return method;
        }

        private static string GetAroundMethodPrefix(MethodDefinition target)
        {
            return $"{Constants.Prefix}around_{target.Name}_{target.MetadataToken.ToUInt32()}_";
        }
    }
}