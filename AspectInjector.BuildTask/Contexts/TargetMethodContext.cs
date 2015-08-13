using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        #region Private Fields

        private static readonly string ExceptionVariableName = "__a$_exception";
        private static readonly string MethodResultVariableName = "__a$_methodResult";

        private readonly ILProcessor _processor;
        private PointCut _exceptionPoint;
        private VariableDefinition _exceptionVar;
        private VariableDefinition _resultVar;

        #endregion Private Fields

        #region Public Constructors

        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            _processor = targetMethod.Body.GetILProcessor();
        }

        #endregion Public Constructors

        #region Public Properties

        public PointCut EntryPoint { get; protected set; }

        public PointCut ExceptionPoint
        {
            get
            {
                if (_exceptionPoint == null)
                    SetupCatchBlock();

                return _exceptionPoint;
            }
        }

        public VariableDefinition ExceptionVariable
        {
            get
            {
                if (_exceptionVar == null)
                    SetupCatchBlock();

                return _exceptionVar;
            }
        }

        public PointCut ExitPoint { get; protected set; }

        public VariableDefinition MethodResultVariable
        {
            get
            {
                if (TargetMethod.ReturnType.IsTypeOf(typeof(void)) && !TargetMethod.IsSetter)
                    return null;

                if (_resultVar == null)
                {
                    //todo:: optimize for compiller generated
                    if (TargetMethod.IsSetter)
                    {
                        var prop = TargetMethod.DeclaringType.Properties.First(p => p.SetMethod == TargetMethod);

                        _resultVar = new VariableDefinition(MethodResultVariableName, prop.GetMethod.ReturnType);
                        OriginalEntryPoint.Processor.Body.Variables.Add(_resultVar);
                        OriginalEntryPoint.Processor.Body.InitLocals = true;
                        OriginalEntryPoint.LoadSelfOntoStack();
                        OriginalEntryPoint.InjectMethodCall(prop.GetMethod, new object[] { });
                        OriginalEntryPoint.SetVariableFromStack(_resultVar);
                    }
                    else
                    {
                        _resultVar = new VariableDefinition(MethodResultVariableName, TargetMethod.ReturnType);
                        OriginalEntryPoint.Processor.Body.Variables.Add(_resultVar);
                        OriginalEntryPoint.Processor.Body.InitLocals = true;
                    }
                }

                return _resultVar;
            }
        }

        public PointCut OriginalCodeReturnPoint { get; protected set; }

        public PointCut OriginalEntryPoint { get; protected set; }

        public PointCut ReturnPoint { get; protected set; }

        public MethodDefinition TargetMethod { get; private set; }

        #endregion Public Properties

        #region Public Methods

        public PointCut FindBaseClassCtorCall()
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

        public virtual void Init()
        {
            SetupEntryPoints();
            SetupReturnPoints();
        }

        #endregion Public Methods

        #region Protected Methods

        protected PointCut GetMethodOriginalEntryPoint()
        {
            var processor = TargetMethod.Body.GetILProcessor();

            if (TargetMethod.Body.Instructions.Count == 1) //if code is optimized
                processor.InsertBefore(TargetMethod.Body.Instructions.First(), processor.Create(OpCodes.Nop));

            return new PointCut(processor, TargetMethod.Body.Instructions.First());
        }

        protected virtual void SetupCatchBlock()
        {
            var exceptionType = TargetMethod.Module.TypeSystem.ResolveType(typeof(Exception));
            _exceptionVar = new VariableDefinition(ExceptionVariableName, exceptionType);
            OriginalCodeReturnPoint.Processor.Body.Variables.Add(_exceptionVar);
            OriginalCodeReturnPoint.Processor.Body.InitLocals = true;

            var setVarInst = OriginalCodeReturnPoint.InsertAfter(OriginalCodeReturnPoint.CreateInstruction(OpCodes.Stloc, _exceptionVar.Index));
            _exceptionPoint = setVarInst.InsertAfter(setVarInst.Processor.Create(OpCodes.Rethrow));

            OriginalCodeReturnPoint = OriginalCodeReturnPoint.Replace(OriginalCodeReturnPoint.Processor.Create(OpCodes.Leave, ReturnPoint.InjectionPoint)); //todo:: optimize

            OriginalCodeReturnPoint.Processor.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = OriginalEntryPoint.InjectionPoint,
                TryEnd = OriginalCodeReturnPoint.InjectionPoint.Next,
                HandlerStart = OriginalCodeReturnPoint.InjectionPoint.Next,
                HandlerEnd = _exceptionPoint.InjectionPoint.Next,
                CatchType = exceptionType
            });
        }

        protected virtual void SetupEntryPoints()
        {
            OriginalEntryPoint = TargetMethod.IsConstructor && !TargetMethod.IsStatic ?
                FindBaseClassCtorCall() :
                GetMethodOriginalEntryPoint();

            EntryPoint = OriginalEntryPoint.InsertBefore(OriginalEntryPoint.Processor.Create(OpCodes.Nop));
        }

        protected virtual void SetupReturnPoints()
        {
            var proc = TargetMethod.Body.GetILProcessor();

            ReturnPoint = new PointCut(proc, proc.Create(OpCodes.Nop));

            OriginalCodeReturnPoint = new PointCut(proc, SetupSingleReturnPoint(proc.Create(OpCodes.Br, ReturnPoint.InjectionPoint))); //todo:: optimize
            proc.SafeAppend(ReturnPoint.InjectionPoint);

            if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                ExitPoint = new PointCut(proc, proc.SafeAppend(proc.CreateOptimized(OpCodes.Ldloc, MethodResultVariable.Index)));
                proc.SafeAppend(proc.Create(OpCodes.Ret));
            }
            else
            {
                ExitPoint = new PointCut(proc, proc.SafeAppend(proc.Create(OpCodes.Ret)));
            }
        }

        protected Instruction SetupSingleReturnPoint(Instruction suggestedSingleReturnPoint)
        {
            var proc = TargetMethod.Body.GetILProcessor();

            var rets = proc.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();

            if (rets.Count == 1)
            {
                if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    proc.SafeInsertBefore(rets.First(), proc.CreateOptimized(OpCodes.Stloc, MethodResultVariable.Index));

                return proc.SafeReplace(rets.First(), suggestedSingleReturnPoint);
            }

            foreach (var i in rets)
            {
                if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    proc.SafeInsertBefore(i, proc.CreateOptimized(OpCodes.Stloc, MethodResultVariable.Index));

                proc.SafeReplace(i, proc.Create(OpCodes.Br, suggestedSingleReturnPoint)); //todo:: optimize
            }

            proc.SafeAppend(suggestedSingleReturnPoint);

            return suggestedSingleReturnPoint;
        }

        #endregion Protected Methods
    }
}