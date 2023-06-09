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

        public AdviceAroundProcess(ILogger log, MethodDefinition target, InjectionDefinition injection) : base(log, target, injection)
        {
            _wrapperNamePrefix = $"{GetAroundMethodPrefix(_method)}w_";
            _unWrapperName = $"{GetAroundMethodPrefix(_method)}u";
            _movedOriginalName = $"{GetAroundMethodPrefix(_method)}o";
        }

        public override void Execute()
        {
            _wrapper = GetNextWrapper();

            _wrapper.Body.Instead(
                (in Cut e) => e
                .LoadAspect(_aspect)
                .Call(_effect.Method, LoadAdviceArgs)
                .Return()
            );
        }

        protected override Cut LoadTargetArgument(in Cut pc, AdviceArgument parameter)
        {
            var targetMethod = CreateRef(GetOrCreateUnwrapper(), pc.Method);
            return pc.ThisOrNull().Call(CreateFuncCtorRef(pc), (in Cut args) => args.Delegate(targetMethod));
        }

        private static MethodReference CreateFuncCtorRef(in Cut cut)
        {
            var mr = new MethodReference(".ctor", cut.TypeSystem.Void, cut.Import(WellKnownTypes.Func_ObjectArray_Object))
            {
                HasThis = true
            };
            mr.Parameters.Add(new ParameterDefinition(cut.TypeSystem.Object));
            mr.Parameters.Add(new ParameterDefinition(cut.TypeSystem.IntPtr));
            return mr;
        }

        protected override Cut LoadArgumentsArgument(in Cut pc, AdviceArgument parameter)
        {
            return pc.Load(_wrapper.Parameters[0]);
        }

        public MethodDefinition GetNextWrapper()
        {
            var prevWrapper = _type.Methods.Where(m => m.Name.StartsWith(_wrapperNamePrefix))
                .Select(m => new { m, i = ushort.Parse(m.Name.Substring(_wrapperNamePrefix.Length)) }).OrderByDescending(g => g.i).FirstOrDefault();

            var newWrapper = CloneDefinition(GetOrCreateUnwrapper());

            newWrapper.IsPrivate = true;
            newWrapper.Name = $"{_wrapperNamePrefix}{(prevWrapper == null ? 0 : prevWrapper.i + 1)}";
            newWrapper.Mark(_module.ImportStandardType(WellKnownTypes.DebuggerHiddenAttribute));
            newWrapper.Mark(_module.ImportStandardType(WellKnownTypes.CompilerGeneratedAttribute));

            RedirectPreviousWrapper(prevWrapper == null ? _method : prevWrapper.m, newWrapper);

            return newWrapper;
        }

        private void RedirectPreviousWrapper(MethodDefinition prev, MethodDefinition next)
        {
            var unwrapper = GetOrCreateUnwrapper();

            var instructions = prev.Body.Instructions.Where(i => i.Operand is MethodReference reference && reference.Resolve() == unwrapper).ToList();

            var nextRef = CreateRef(next, prev);

            foreach (var inst in instructions)
                inst.Operand = nextRef;
        }

        private MethodDefinition GetOrCreateUnwrapper()
        {
            var unwrapper = _type.Methods.FirstOrDefault(m => m.Name == _unWrapperName);
            if (unwrapper != null)
                return unwrapper;

            unwrapper = CloneDefinition(_method);

            unwrapper.Name = _unWrapperName;
            unwrapper.ReturnType = _module.TypeSystem.Object;
            unwrapper.IsPrivate = true;
            unwrapper.Parameters.Clear();
            var argsParam = new ParameterDefinition(_module.ImportStandardType(WellKnownTypes.Object_Array));
            unwrapper.Parameters.Add(argsParam);
            unwrapper.Body.InitLocals = true;
            unwrapper.Mark(_module.ImportStandardType(WellKnownTypes.DebuggerHiddenAttribute));
            unwrapper.Mark(_module.ImportStandardType(WellKnownTypes.CompilerGeneratedAttribute));

            var original = WrapEntryPoint(unwrapper);

            unwrapper.Body.Instead((in Cut il) => WriteUnwrapperBody(il, argsParam, original));

            return unwrapper;
        }

        private Cut WriteUnwrapperBody(in Cut cut, ParameterDefinition argsParam, MethodDefinition original)
        {
            var refList = new List<Tuple<int, VariableDefinition>>();

            var il = cut;

            var originalRef = CreateRef(original, il.Method);

            il = il.ThisOrStatic().Call(originalRef, (in Cut call_cut) =>
            {
                var c = call_cut;
                for (int i = 0; i < original.Parameters.Count; i++)
                {
                    var p = original.Parameters[i];

                    if (p.ParameterType.IsByReference)
                    {
                        var elementType = ((ByReferenceType)p.ParameterType).ElementType;

                        var tempVar = new VariableDefinition(elementType);
                        refList.Add(new Tuple<int, VariableDefinition>(i, tempVar));
                        c.Method.Body.Variables.Add(tempVar);

                        c = c
                        .Store(tempVar, (in Cut v) => v
                            .Load(argsParam)
                            .GetByIndex(v.TypeSystem.Object, i)
                            .Cast(v.TypeSystem.Object, elementType))
                        .LoadRef(tempVar);
                    }
                    else
                    {
                        c = c
                        .Load(argsParam)
                        .GetByIndex(c.TypeSystem.Object, i)
                        .Cast(c.TypeSystem.Object, p.ParameterType);
                    }
                }

                return c;
            });

            foreach (var refPar in refList)
                il = il.Load(argsParam).SetByIndex(il.TypeSystem.Object, refPar.Item1, (in Cut val) => val.Load(refPar.Item2).Cast(refPar.Item2.VariableType, val.TypeSystem.Object));

            if (original.ReturnType.Match(original.Module.TypeSystem.Void))
                il = il.Value(null);
            else
                il = il.Cast(originalRef.ReturnType, il.Method.ReturnType);

            return il.Return();
        }

        private MethodDefinition WrapEntryPoint(MethodDefinition unwrapper)
        {
            var original = CloneDefinition(_method);

            original.Name = _movedOriginalName;
            original.IsPrivate = true;

            var returnType = _method.ReturnType;

            MoveBody(_method, original);

            _method.Mark(_module.ImportStandardType(WellKnownTypes.DebuggerStepThroughAttribute));

            _method.Body.Append(
                (in Cut cut) =>
                {
                    var e = cut;
                    ////var args = null;
                    var argsVar = new VariableDefinition(_module.ImportStandardType(WellKnownTypes.Object_Array));
                    _method.Body.Variables.Add(argsVar);
                    _method.Body.InitLocals = true;

                    ////args = new object[] { param1, param2 ...};
                    e = e.Store(argsVar, (in Cut args) => base.LoadArgumentsArgument(args, null));

                    var unwrapperRef = CreateRef(unwrapper, _method);

                    //// Unwrapper(args);
                    e = e.ThisOrStatic().Call(unwrapperRef, (in Cut args) => args.Load(argsVar));

                    // proxy ref and out params
                    for (int i = 0; i < _method.Parameters.Count; i++)
                    {
                        var p = _method.Parameters[i];
                        if (p.ParameterType.IsByReference)
                            e = e.Store(p, (in Cut val) => val.Load(argsVar).GetByIndex(val.TypeSystem.Object, i).Cast(val.TypeSystem.Object, p.ParameterType));
                    }

                    //drop if void, cast if not is object
                    if (returnType.Match(cut.TypeSystem.Void))
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
            var tbProc = to.Body.GetILProcessor();
            var fbProc = from.Body.GetILProcessor();
            var fdbg = from.DebugInformation;
            var fsp = from.DebugInformation.SequencePoints;
            var tsp = to.DebugInformation.SequencePoints;

            to.HasThis = from.HasThis;

            var codeStart = from.Body.GetUserCodeStart();
            var init = codeStart == null ? 0 : frb.IndexOf(codeStart);

            foreach (var inst in frb.Skip(init).ToList())
            {
                var sp = fdbg.GetSequencePoint(inst);

                // Cecil doens't allow to remove last instruction
                if (frb.Count == 1)
                    fbProc.Clear();
                else
                    frb.Remove(inst);

                tbProc.Append(inst);
                if (sp != null)
                {
                    tsp.Add(new SequencePoint(inst, sp.Document) { EndColumn = sp.EndColumn, EndLine = sp.EndLine, StartColumn = sp.StartColumn, StartLine = sp.StartLine });
                    fsp.Remove(sp);
                }
            }

            if (from.DebugInformation.Scope != null)
            {
                to.DebugInformation.Scope = from.DebugInformation.Scope;
                to.DebugInformation.Scope.Start = new InstructionOffset(to.Body.Instructions[0].Offset);
                to.DebugInformation.Scope.End = new InstructionOffset(to.Body.Instructions[to.Body.Instructions.Count - 1].Offset);
                from.DebugInformation.Scope = null;
            }


            //to.DebugInformation.StateMachineKickOffMethod = from;

            if (from.DebugInformation.StateMachineKickOffMethod != null)
            {
                to.DebugInformation.StateMachineKickOffMethod = from.DebugInformation.StateMachineKickOffMethod;
                from.DebugInformation.StateMachineKickOffMethod = null;
            }

            if (from.DebugInformation.HasCustomDebugInformations)
                foreach (var cdi in from.DebugInformation.CustomDebugInformations.ToArray())
                {
                    from.DebugInformation.CustomDebugInformations.Remove(cdi);
                    to.DebugInformation.CustomDebugInformations.Add(cdi);
                }

            //var cstm = new StateMachineScopeDebugInformation();
            //cstm.Scopes.Add(new StateMachineScope(to.Body.Instructions[0], to.Body.Instructions[to.Body.Instructions.Count - 1]));
            //to.CustomDebugInformations.Add(cstm);

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

        private MethodDefinition CloneDefinition(MethodDefinition origin)
        {
            var method = new MethodDefinition(origin.Name, origin.Attributes & ~MethodAttributes.RTSpecialName, origin.ReturnType);

            _type.Methods.Add(method);

            origin.GenericParameters.CloneTo(method);

            if (origin.ReturnType is GenericParameter gp && gp.Owner == origin)
                method.ReturnType = _type.Module.ImportReference(method.GenericParameters[gp.Position]);

            if (origin.IsSpecialName)
                method.IsSpecialName = true;

            foreach (var parameter in origin.Parameters)
            {
                var paramType = parameter.ParameterType;
                if (paramType is GenericParameter gpp)
                {
                    if (gpp.Owner == origin)
                        paramType = method.GenericParameters[gpp.Position];
                    else if (gpp.Owner != _type)
                        throw new InvalidOperationException("Cannot clone definition of external method.");
                }

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
