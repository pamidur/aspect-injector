using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        #region Protected Fields

        protected static readonly string ExceptionVariableName = "__a$_exception";
        protected static readonly string MethodResultVariableName = "__a$_methodResult";

        protected readonly ILProcessor Processor;

        #endregion Protected Fields

        #region Private Fields

        private PointCut _entryPoint;
        private PointCut _originalEntryPoint;
        private PointCut _originalReturnPoint;
        private VariableDefinition _resultVar;
        private PointCut _returnPoint;

        #endregion Private Fields

        #region Public Constructors

        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            Processor = targetMethod.Body.GetILProcessor();
        }

        #endregion Public Constructors

        #region Public Properties

        public virtual PointCut EntryPoint
        {
            get
            {
                if (_entryPoint == null)
                    SetupEntryPoints();

                return _entryPoint;
            }
        }

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

        #endregion Public Properties

        public PointCut GetAroundPoint()
        {
            throw new NotImplementedException();
        }

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
                    && ((MethodReference)i.Operand).DeclaringType.IsTypeOf(md.DeclaringType.BaseType));

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

            var constructor = TargetMethod.Module.Import(typeof(CompilerGeneratedAttribute)).Resolve() //todo:: import to exact member's module
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