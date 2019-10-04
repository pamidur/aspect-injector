using AspectInjector.Core.Advice.Effects;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
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
        private MethodDefinition _entry;

        public AdviceAroundProcess(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _entry = target;
            _wrapperNamePrefix = $"{GetAroundMethodPrefix(_method)}w_";
            _unWrapperName = $"{GetAroundMethodPrefix(_method)}u";
            _movedOriginalName = $"{GetAroundMethodPrefix(_method)}o";
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
            var targetMethod = CreateRef(GetOrCreateUnwrapper(), pc.Method);
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
            var prevWrapper = _type.Methods.Where(m => m.Name.StartsWith(_wrapperNamePrefix))
                .Select(m => new { m, i = ushort.Parse(m.Name.Substring(_wrapperNamePrefix.Length)) }).OrderByDescending(g => g.i).FirstOrDefault();

            var newWrapper = DuplicateMethodDefinition(GetOrCreateUnwrapper());
            newWrapper.IsPrivate = true;
            newWrapper.Name = $"{_wrapperNamePrefix}{(prevWrapper == null ? 0 : prevWrapper.i + 1)}";
            newWrapper.Mark(WellKnownTypes.DebuggerHiddenAttribute);

            RedirectPreviousWrapper(prevWrapper == null ? _method : prevWrapper.m, newWrapper);

            return newWrapper;
        }

        private void RedirectPreviousWrapper(MethodDefinition prev, MethodDefinition next)
        {
            var unwrapper = GetOrCreateUnwrapper();

            var instructions = prev.Body.Instructions.Where(i => i.Operand is MethodReference && ((MethodReference)i.Operand).Resolve() == unwrapper).ToList();

            var nextRef = CreateRef(next, prev);
       
            foreach (var inst in instructions)
                inst.Operand = nextRef;
        }

        private MethodDefinition GetOrCreateUnwrapper()
        {
            var unwrapper = _type.Methods.FirstOrDefault(m => m.Name == _unWrapperName);
            if (unwrapper != null)
                return unwrapper;

            unwrapper = DuplicateMethodDefinition(_method);
            unwrapper.Name = _unWrapperName;
            unwrapper.ReturnType = _module.ImportReference(StandardTypes.Object);
            unwrapper.IsPrivate = true;
            unwrapper.Parameters.Clear();
            var argsParam = new ParameterDefinition(_module.ImportReference(StandardTypes.ObjectArray));
            unwrapper.Parameters.Add(argsParam);
            unwrapper.Body.InitLocals = true;
            unwrapper.Mark(WellKnownTypes.DebuggerHiddenAttribute);

            var original = WrapEntryPoint(unwrapper);

            unwrapper.Body.Instead(
                il =>
                {
                    var refList = new List<Tuple<int, VariableDefinition>>();

                    var originalRef = CreateRef(original, unwrapper);
                  
                    il = il.ThisOrStatic().Call(originalRef, c =>
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
                        il = il.Cast(originalRef.ReturnType, unwrapper.ReturnType);

                    return il.Return();
                });

            return unwrapper;
        }

        private MethodDefinition WrapEntryPoint(MethodDefinition unwrapper)
        {
            var original = DuplicateMethodDefinition(_method);


            original.Name = _movedOriginalName;
            original.IsPrivate = true;

            //var returnType = _method.ResolveIfGeneric(_method.ReturnType);

            var returnType = _method.ReturnType;

            MoveBody(_method, original);

            _method.Body.Append(
                e =>
                {
                    //var args = null;
                    var argsVar = new VariableDefinition(_module.ImportReference(StandardTypes.ObjectArray));
                    _method.Body.Variables.Add(argsVar);
                    _method.Body.InitLocals = true;

                    //args = new object[] { param1, param2 ...};
                    e = e.Store(argsVar, args => base.LoadArgumentsArgument(args, null));

                    var unwrapperRef = CreateRef(unwrapper, _method);

                    // Unwrapper(args);
                    e = e.ThisOrStatic().Call(unwrapperRef, args => args.Load(argsVar));

                    // proxy ref and out params
                    for (int i = 0; i < _method.Parameters.Count; i++)
                    {
                        var p = _method.Parameters[i];
                        if (p.ParameterType.IsByReference)
                            e = e.Store(p, val => val.Load(argsVar).GetByIndex(StandardTypes.Object, i).Cast(StandardTypes.Object, p.ParameterType));
                    }

                    //drop if void, cast if not is object
                    if (returnType.Match(StandardTypes.Void))
                        e = e.Pop();
                    else
                        e = e.Cast(unwrapperRef.ReturnType, returnType);

                    return e.Return();
                });

            return original;
        }

        private MethodReference CreateRef(MethodDefinition definition, MethodDefinition callSite)
        {
            TypeReference type = _type;
            if (type.HasGenericParameters)
                type = type.MakeGenericInstanceType(type.GenericParameters.ToArray());

            MethodReference reference = definition.MakeReference(type);
            if (callSite.HasGenericParameters)
                reference = reference.MakeGenericInstanceMethod(callSite.GenericParameters.ToArray());

            return reference;
        }

        private void MoveBody(MethodDefinition from, MethodDefinition to)
        {
            var frb = from.Body.Instructions;
            var trb = to.Body.Instructions;
            var fdbg = from.DebugInformation;
            var fsp = from.DebugInformation.SequencePoints;
            var tsp = to.DebugInformation.SequencePoints;

            var codeStart = from.Body.GetUserCodeStart();
            var init = codeStart == null ? 0 : frb.IndexOf(codeStart);

            foreach (var inst in frb.Skip(init).ToList())
            {
                var sp = fdbg.GetSequencePoint(inst);
                if (sp != null) fsp.Remove(sp);

                frb.Remove(inst);
                trb.Add(inst);

                if (sp != null)
                    tsp.Add(new SequencePoint(inst, sp.Document) { EndColumn = sp.EndColumn, EndLine = sp.EndLine, StartColumn = sp.StartColumn, StartLine = sp.StartLine });
            }

            to.DebugInformation.Scope = from.DebugInformation.Scope;

            var to_vars = to.Body.Variables;
            foreach (var var in from.Body.Variables)
                to_vars.Add(new VariableDefinition(to.Module.ImportReference(var.VariableType)));
            from.Body.Variables.Clear();

            if (to.Body.HasVariables)
                to.Body.InitLocals = true;

            var to_handlers = to.Body.ExceptionHandlers;
            foreach (var handler in from.Body.ExceptionHandlers)
                to_handlers.Add(handler);
            from.Body.ExceptionHandlers.Clear();
        }

        private MethodDefinition DuplicateMethodDefinition(MethodDefinition origin)
        {
            var method = new MethodDefinition(origin.Name,
               origin.Attributes & ~MethodAttributes.RTSpecialName,
               origin.ReturnType);

            origin.DeclaringType.Methods.Add(method);

            foreach (var gparam in origin.GenericParameters)
                method.GenericParameters.Add(gparam.Clone(method));

            if (origin.ReturnType is GenericParameter gp && gp.Owner == origin)
                method.ReturnType = _module.ImportReference(method.GenericParameters[gp.Position]);

            if (origin.IsSpecialName)
                method.IsSpecialName = true;

            foreach (var parameter in origin.Parameters)
            {
                var paramType = parameter.ParameterType;
                if (paramType is GenericParameter gpp && gpp.Owner == origin)
                    paramType = method.GenericParameters[gpp.Position];

                method.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, _module.ImportReference(paramType)));
            }

            return method;
        }

        private static string GetAroundMethodPrefix(MethodDefinition target)
        {
            return $"{Constants.Prefix}around_{target.Name}_{target.MetadataToken.ToUInt32()}_";
        }
    }
}