using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        #region Fields

        protected static readonly string MethodResultVariableName = "__a$_methodResult";
        protected static readonly string MethodArgsVariableName = "__a$_methodArgs";
        protected static readonly string AroundWrappedMethodPrefix = "__a$w";
        protected static readonly string AroundOriginalMethodPrefix = "__a$o_";
        protected static readonly string AroundUnwrappedMethodPrefix = "__a$u_";

        protected readonly ILProcessor Processor;

        private PointCut _entryPoint;
        private PointCut _originalEntryPoint;  //<-- original method starts here
        private PointCut _originalReturnPoint; //<-- original method ends here
        private PointCut _returnPoint;

        private VariableDefinition _resultVar;
        private VariableDefinition _argsVar;

        private PointCut _topWrapperCallSite;

        private int _wrapperNo;
        private MethodDefinition _lastWrapper;

        #endregion Fields

        #region Constructors

        public TargetMethodContext(MethodDefinition targetMethod, ModuleContext moduleContext)
        {
            ModuleContext = moduleContext;

            TargetMethod = targetMethod;
            Processor = targetMethod.Body.GetILProcessor();
        }

        #endregion Constructors

        #region Properties

        public virtual PointCut EntryPoint
        {
            get
            {
                if (_entryPoint == null)
                    SetupEntryPoints();

                return _entryPoint;
            }
        }

        public MethodDefinition TopWrapper { get; private set; }

        public virtual VariableDefinition MethodResultVariable
        {
            get
            {
                if (TargetMethod.ReturnType.IsTypeOf(typeof(void)) && !TargetMethod.IsSetter)
                    return null;

                if (_resultVar == null)
                    SetupReturnPoints();

                return _resultVar;
            }
        }

        public virtual VariableDefinition ArgsVariable
        {
            get
            {
                if (_argsVar == null)
                {
                    EntryPoint.LoadCallArgument(Processor.Body.Method.Parameters.ToArray(), TypeSystem.ObjectArray);
                    _argsVar = EntryPoint.CreateVariableFromStack(TypeSystem.ObjectArray, MethodArgsVariableName);

                    //foreach (var param in TargetMethod.Parameters)
                    //{
                    //    // todo:: optimize and move to some syntax helper
                    //    if (param.ParameterType.IsByReference)
                    //    {
                    //        OriginalEntryPoint.LoadParameterOntoStack(param, param.ParameterType);

                    //        OriginalEntryPoint.LoadVariableOntoStack(_argsVar);
                    //        OriginalEntryPoint.InsertBefore(OriginalEntryPoint.CreateInstruction(OpCodes.Ldc_I4, param.Index));
                    //        OriginalEntryPoint.InsertBefore(OriginalEntryPoint.CreateInstruction(OpCodes.Ldelem_Ref));

                    //        var elementType = ((ByReferenceType)param.ParameterType).ElementType;
                    //        OriginalEntryPoint.BoxUnboxTryCastIfNeeded(TypeSystem.Object, elementType);
                    //        OriginalEntryPoint.SaveIndirect(elementType);
                    //    }
                    //    else
                    //    {
                    //        OriginalEntryPoint.LoadVariableOntoStack(_argsVar);
                    //        OriginalEntryPoint.InsertBefore(OriginalEntryPoint.CreateInstruction(OpCodes.Ldc_I4, param.Index));
                    //        OriginalEntryPoint.InsertBefore(OriginalEntryPoint.CreateInstruction(OpCodes.Ldelem_Ref));

                    //        OriginalEntryPoint.BoxUnboxTryCastIfNeeded(TypeSystem.Object, param.ParameterType);
                    //        OriginalEntryPoint.InsertBefore(OriginalEntryPoint.CreateInstruction(OpCodes.Starg, TargetMethod.HasThis ? param.Index + 1 : param.Index));
                    //    }
                    //}
                }

                return _argsVar;
            }
        }

        public virtual PointCut OriginalEntryPoint
        {
            get
            {
                if (_originalEntryPoint == null)
                    SetupEntryPoints();

                return _originalEntryPoint;
            }
        }

        public virtual PointCut OriginalReturnPoint
        {
            get
            {
                if (_originalReturnPoint == null)
                    SetupReturnPoints();

                return _originalReturnPoint;
            }
        }

        public virtual PointCut ReturnPoint
        {
            get
            {
                if (_returnPoint == null)
                    SetupReturnPoints();

                return _returnPoint;
            }
        }

        public MethodDefinition TargetMethod { get; private set; }

        protected ModuleContext ModuleContext { get; private set; }

        protected ExtendedTypeSystem TypeSystem { get { return ModuleContext.TypeSystem; } }

        #endregion Properties

        #region Methods

        public PointCut CreateNewAroundPoint()
        {
            if (_topWrapperCallSite == null)
                SetupAroundInfrastructure();

            var newWapper = new MethodDefinition(AroundWrappedMethodPrefix + _wrapperNo + "_" + TopWrapper.Name,
                TargetMethod.Attributes,
                TypeSystem.Object);

            newWapper.NoInlining = false;

            MarkDebuggerHidden(newWapper);
            MarkCompilerGenerated(newWapper);

            TargetMethod.DeclaringType.Methods.Add(newWapper);

            var argsParam = new ParameterDefinition(TypeSystem.ObjectArray);
            newWapper.Parameters.Add(argsParam);

            var tempPc = PointCut.FromEmptyBody(newWapper.Body, OpCodes.Ret);

            //if (_topWrapper.ReturnType == TypeSystem.Void)
            //    tempPc.CreateVariableFromStack(TypeSystem.Object);
            //else if (_topWrapper.ReturnType.IsValueType)
            //    tempPc.InsertBefore(tempPc.CreateInstruction(OpCodes.Box, TargetMethod.Module.Import(_topWrapper.ReturnType)));

            var newWapperPoint = new WrapperPointCut(argsParam, _lastWrapper, newWapper.Body.GetILProcessor(), newWapper.Body.Instructions.First());

            _lastWrapper = newWapper;
            _wrapperNo++;

            _topWrapperCallSite.InjectionPoint.Operand = newWapper;

            return newWapperPoint;
        }

        #endregion Methods

        #region Protected Methods

        protected PointCut FindBaseClassCtorCall()
        {
            var md = TargetMethod;
            var proc = md.Body.GetILProcessor();

            if (!md.IsConstructor)
                throw new Exception(md.ToString() + " is not ctor.");

            if (md.DeclaringType.IsValueType)
                return new PointCut(proc, md.Body.Instructions.First());

            var point = md.Body.Instructions.FirstOrDefault(
                i => i != null && i.OpCode == OpCodes.Call && i.Operand is MethodReference
                    && ((MethodReference)i.Operand).Resolve().IsConstructor
                    && (((MethodReference)i.Operand).DeclaringType.IsTypeOf(md.DeclaringType.BaseType)
                        || ((MethodReference)i.Operand).DeclaringType.IsTypeOf(md.DeclaringType)));

            if (point == null)
                throw new Exception("Cannot find base class ctor call");

            return new PointCut(proc, point.Next);
        }

        protected PointCut GetMethodOriginalEntryPoint()
        {
            var processor = TargetMethod.Body.GetILProcessor();

            if (TargetMethod.Body.Instructions.Count == 1) //if code is optimized
                processor.InsertBefore(TargetMethod.Body.Instructions.First(), processor.Create(OpCodes.Nop));

            return new PointCut(processor, TargetMethod.Body.Instructions.First());
        }

        protected void MarkCompilerGenerated(ICustomAttributeProvider member)
        {
            if (member.CustomAttributes.Any(ca => ca.IsAttributeOfType<CompilerGeneratedAttribute>()))
                return;

            var constructor = TypeSystem.CompilerGeneratedAttribute.Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            member.CustomAttributes.Add(new CustomAttribute(TargetMethod.Module.Import(constructor)));
        }

        protected void MarkDebuggerStepThrough(ICustomAttributeProvider member)
        {
            if (member.CustomAttributes.Any(ca => ca.IsAttributeOfType<DebuggerStepThroughAttribute>()))
                return;

            var constructor = TypeSystem.DebuggerStepThroughAttribute.Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            member.CustomAttributes.Add(new CustomAttribute(TargetMethod.Module.Import(constructor)));
        }

        protected void MarkDebuggerHidden(ICustomAttributeProvider member)
        {
            if (member.CustomAttributes.Any(ca => ca.IsAttributeOfType<DebuggerHiddenAttribute>()))
                return;

            var constructor = TypeSystem.DebuggerHiddenAttribute.Resolve()
                .Methods.First(m => m.IsConstructor && !m.IsStatic);

            member.CustomAttributes.Add(new CustomAttribute(TargetMethod.Module.Import(constructor)));
        }

        protected Instruction SetupSingleReturnPoint(Instruction suggestedSingleReturnPoint, VariableReference resultVar)
        {
            var proc = TargetMethod.Body.GetILProcessor();

            var rets = proc.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();

            if (rets.Count == 1)
            {
                if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    proc.SafeInsertBefore(rets.First(), proc.CreateOptimized(OpCodes.Stloc, resultVar.Index));

                return proc.SafeReplace(rets.First(), suggestedSingleReturnPoint);
            }

            foreach (var i in rets)
            {
                if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    proc.SafeInsertBefore(i, proc.CreateOptimized(OpCodes.Stloc, resultVar.Index));

                proc.SafeReplace(i, proc.Create(OpCodes.Br, suggestedSingleReturnPoint)); //todo:: optimize
            }

            proc.SafeAppend(suggestedSingleReturnPoint);

            return suggestedSingleReturnPoint;
        }

        #endregion Protected Methods

        #region Private Methods

        private MethodDefinition CreateUnwrapMethod(MethodDefinition originalMethod)
        {
            var unwrapMethod = new MethodDefinition(AroundUnwrappedMethodPrefix + originalMethod.Name,
                originalMethod.Attributes,
                TypeSystem.Object);

            unwrapMethod.NoInlining = false;

            MarkDebuggerHidden(unwrapMethod);
            MarkCompilerGenerated(unwrapMethod);

            originalMethod.DeclaringType.Methods.Add(unwrapMethod);

            var argsParam = new ParameterDefinition(TypeSystem.MakeArrayType(TypeSystem.Object));
            unwrapMethod.Parameters.Add(argsParam);

            var unwrapPoint = PointCut.FromEmptyBody(unwrapMethod.Body, OpCodes.Ret);

            unwrapPoint.LoadSelfOntoStack();

            var vars = new List<VariableDefinition>();

            foreach (var parameter in originalMethod.Parameters)
            {
                unwrapPoint.LoadParameterOntoStack(argsParam);
                unwrapPoint.InsertBefore(unwrapPoint.CreateInstruction(OpCodes.Ldc_I4, originalMethod.Parameters.IndexOf(parameter)));
                unwrapPoint.InsertBefore(unwrapPoint.CreateInstruction(OpCodes.Ldelem_Ref));

                var varType = parameter.ParameterType;

                if (varType.IsByReference)
                    varType = ((ByReferenceType)varType).ElementType;

                unwrapPoint.BoxUnboxTryCastIfNeeded(TypeSystem.Object, varType);

                vars.Add(unwrapPoint.CreateVariableFromStack(varType));
            }

            foreach (var parameter in originalMethod.Parameters)
                unwrapPoint.LoadVariableOntoStack(vars[originalMethod.Parameters.IndexOf(parameter)], parameter.ParameterType);

            unwrapPoint.InsertBefore(unwrapPoint.CreateInstruction(OpCodes.Call, TargetMethod.Module.Import(TargetMethod)));

            if (originalMethod.ReturnType == TypeSystem.Void)
                unwrapPoint.LoadValueOntoStack<object>(null);
            else if (originalMethod.ReturnType.IsValueType)
                unwrapPoint.InsertBefore(unwrapPoint.CreateInstruction(OpCodes.Box, TargetMethod.Module.Import(originalMethod.ReturnType)));

            foreach (var parameter in originalMethod.Parameters)
            {
                var index = originalMethod.Parameters.IndexOf(parameter);

                unwrapPoint.LoadParameterOntoStack(argsParam);
                unwrapPoint.InsertBefore(unwrapPoint.CreateInstruction(OpCodes.Ldc_I4, index));
                unwrapPoint.LoadVariableOntoStack(vars[index], TypeSystem.Object);
                unwrapPoint.InsertBefore(unwrapPoint.CreateInstruction(OpCodes.Stelem_Ref));
            }

            return unwrapMethod;
        }

        private MethodDefinition WrapOriginalMethod()
        {
            var originalMethod = new MethodDefinition(AroundOriginalMethodPrefix + TargetMethod.Name,
                TargetMethod.Attributes,
                TargetMethod.ReturnType);

            //MarkDebuggerStepThrough(originalMethod);
            ///MarkCompilerGenerated(originalMethod);

            foreach (var inst in TargetMethod.Body.Instructions)
                originalMethod.Body.Instructions.Add(inst);

            foreach (var var in TargetMethod.Body.Variables)
                originalMethod.Body.Variables.Add(new VariableDefinition(var.Name, var.VariableType));

            if (originalMethod.Body.HasVariables)
                originalMethod.Body.InitLocals = true;

            foreach (var handler in TargetMethod.Body.ExceptionHandlers)
                originalMethod.Body.ExceptionHandlers.Add(handler);

            foreach (var parameter in TargetMethod.Parameters)
                originalMethod.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, TargetMethod.Module.Import(parameter.ParameterType)));

            TargetMethod.DeclaringType.Methods.Add(originalMethod);

            //erase old body
            TargetMethod.Body.Instructions.Clear();
            TargetMethod.Body = new MethodBody(TargetMethod);

            var wapper = TargetMethod;
            TargetMethod = originalMethod;

            return wapper;
        }

        private void SetupAroundInfrastructure()
        {
            TopWrapper = WrapOriginalMethod();
            _lastWrapper = CreateUnwrapMethod(TopWrapper);

            var topWrapperCut = PointCut.FromEmptyBody(TopWrapper.Body, OpCodes.Ret);

            //var args = new object[]{ arg1, agr2 }
            topWrapperCut.LoadCallArgument(TopWrapper.Parameters.ToArray(), TypeSystem.ObjectArray);
            var argsvar = topWrapperCut.CreateVariableFromStack(TypeSystem.ObjectArray);

            // ExecExternalWrapper
            topWrapperCut.LoadSelfOntoStack();
            _topWrapperCallSite = topWrapperCut.InjectMethodCall(_lastWrapper, new object[] { argsvar });

            var resultVar = topWrapperCut.CreateVariableFromStack(TypeSystem.Object);

            //fill ref and out
            foreach (var param in TopWrapper.Parameters)
            {
                var index = TopWrapper.Parameters.IndexOf(param);

                // todo:: optimize and move to some syntax helper
                if (param.ParameterType.IsByReference)
                {
                    topWrapperCut.LoadParameterOntoStack(param, param.ParameterType);

                    topWrapperCut.LoadVariableOntoStack(argsvar);
                    topWrapperCut.InsertBefore(topWrapperCut.CreateInstruction(OpCodes.Ldc_I4, index));
                    topWrapperCut.InsertBefore(topWrapperCut.CreateInstruction(OpCodes.Ldelem_Ref));

                    var elementType = ((ByReferenceType)param.ParameterType).ElementType;
                    topWrapperCut.BoxUnboxTryCastIfNeeded(TypeSystem.Object, elementType);
                    topWrapperCut.SaveIndirect(elementType);
                }
            }

            if (TopWrapper.ReturnType != TypeSystem.Void)
            {
                topWrapperCut.LoadVariableOntoStack(resultVar);
                topWrapperCut.BoxUnboxTryCastIfNeeded(TypeSystem.Object, TargetMethod.ReturnType);
            }
        }

        private void SetupEntryPoints()
        {
            if (_originalEntryPoint != null || _entryPoint != null)
                throw new InvalidOperationException("Something went wrong");

            _originalEntryPoint = TargetMethod.IsConstructor && !TargetMethod.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            _entryPoint = OriginalEntryPoint.InsertBefore(Processor.Create(OpCodes.Nop));
        }

        private void SetupReturnPoints()
        {
            if (_returnPoint != null || _originalReturnPoint != null)
                throw new InvalidOperationException("Something went wrong");

            SetupReturnVariable();

            var singleReturnPoint = Processor.Create(OpCodes.Nop);
            _originalReturnPoint = new PointCut(Processor, SetupSingleReturnPoint(Processor.Create(OpCodes.Br, singleReturnPoint), MethodResultVariable)); //todo:: optimize
            Processor.SafeAppend(singleReturnPoint);

            if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                _returnPoint = new PointCut(Processor, Processor.SafeAppend(Processor.CreateOptimized(OpCodes.Ldloc, MethodResultVariable.Index)));
                Processor.SafeAppend(Processor.Create(OpCodes.Ret));
            }
            else
            {
                _returnPoint = new PointCut(Processor, Processor.SafeAppend(Processor.Create(OpCodes.Ret)));
            }
        }

        private void SetupReturnVariable()
        {
            if (_resultVar != null)
                throw new InvalidOperationException("Something went wrong");

            if (TargetMethod.ReturnType.IsTypeOf(typeof(void)) && !TargetMethod.IsSetter)
                return;

            //todo:: optimize for compiller generated (get backing field)
            if (TargetMethod.IsSetter)
            {
                var prop = TargetMethod.DeclaringType.Properties.First(p => p.SetMethod == TargetMethod);

                EntryPoint.LoadSelfOntoStack();
                EntryPoint.InjectMethodCall(prop.GetMethod, new object[] { });

                _resultVar = EntryPoint.CreateVariableFromStack(prop.GetMethod.ReturnType, MethodResultVariableName);
            }
            else
            {
                _resultVar = OriginalEntryPoint.CreateVariable(TargetMethod.ReturnType, MethodResultVariableName);
            }
        }

        #endregion Private Methods
    }
}