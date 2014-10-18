using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class TargetMethodContext
    {
        private static readonly string _exceptionVariableName = "__a$_exception";
        private static readonly string _methodResultVariableName = "__a$_method_result";

        private Instruction _exceptionPoint;
        private VariableDefinition _exceptionVar;
        private VariableDefinition _resultVar;

        public TargetMethodContext(MethodDefinition targetMethod)
        {
            TargetMethod = targetMethod;
            Processor = TargetMethod.Body.GetILProcessor();

            SetupEntryPoints();
            SetupReturnPoints();
        }

        public Instruction EntryPoint { get; private set; }

        public Instruction ExceptionPoint
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

        public Instruction ExitPoint { get; private set; }

        public VariableDefinition MethodResultVariable
        {
            get
            {
                if (TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    return null;

                if (_resultVar == null)
                {
                    _resultVar = new VariableDefinition(_methodResultVariableName, TargetMethod.ReturnType);
                    Processor.Body.Variables.Add(_resultVar);
                    Processor.Body.InitLocals = true;
                }

                return _resultVar;
            }
        }

        public Instruction OriginalCodeReturnPoint { get; private set; }

        public Instruction OriginalEntryPoint { get; private set; }

        public ILProcessor Processor { get; private set; }

        public Instruction ReturnPoint { get; private set; }

        public MethodDefinition TargetMethod { get; private set; }

        private void SetupCatchBlock()
        {
            var exceptionType = TargetMethod.Module.TypeSystem.ResolveType(typeof(Exception));
            _exceptionVar = new VariableDefinition(_exceptionVariableName, exceptionType);
            Processor.Body.Variables.Add(_exceptionVar);
            Processor.Body.InitLocals = true;

            var setVarInst = Processor.SafeInsertAfter(OriginalCodeReturnPoint, Processor.CreateOptimized(OpCodes.Stloc, _exceptionVar.Index));
            _exceptionPoint = Processor.SafeInsertAfter(setVarInst, Processor.Create(OpCodes.Rethrow));

            OriginalCodeReturnPoint = Processor.SafeReplace(OriginalCodeReturnPoint, Processor.Create(OpCodes.Leave, ReturnPoint)); //todo:: optimize

            Processor.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Catch)
            {
                TryStart = OriginalEntryPoint,
                TryEnd = OriginalCodeReturnPoint.Next,
                HandlerStart = OriginalCodeReturnPoint.Next,
                HandlerEnd = _exceptionPoint.Next,
                CatchType = exceptionType
            });
        }

        private void SetupEntryPoints()
        {
            OriginalEntryPoint = TargetMethod.IsConstructor ?
                TargetMethod.FindBaseClassCtorCall() :
                TargetMethod.Body.Instructions.First();

            EntryPoint = Processor.SafeInsertBefore(OriginalEntryPoint, Processor.Create(OpCodes.Nop));
        }

        private void SetupReturnPoints()
        {
            ReturnPoint = Processor.Create(OpCodes.Nop);

            OriginalCodeReturnPoint = SetupSingleReturnPoint(Processor.Create(OpCodes.Br, ReturnPoint));//todo:: optimize
            Processor.SafeAppend(ReturnPoint);

            if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
            {
                ExitPoint = Processor.SafeAppend(Processor.CreateOptimized(OpCodes.Ldloc, MethodResultVariable.Index));
                Processor.SafeAppend(Processor.Create(OpCodes.Ret));
            }
            else
            {
                ExitPoint = Processor.SafeAppend(Processor.Create(OpCodes.Ret));
            }
        }

        private Instruction SetupSingleReturnPoint(Instruction suggestedSingleReturnPoint)
        {
            var rets = Processor.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();

            if (rets.Count == 1)
            {
                if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    Processor.SafeInsertBefore(rets.First(), Processor.CreateOptimized(OpCodes.Stloc, MethodResultVariable.Index));

                return Processor.SafeReplace(rets.First(), suggestedSingleReturnPoint);
            }

            foreach (var i in rets)
            {
                if (!TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    Processor.SafeInsertBefore(rets.First(), Processor.CreateOptimized(OpCodes.Stloc, MethodResultVariable.Index));

                Processor.SafeReplace(i, Processor.Create(OpCodes.Br, suggestedSingleReturnPoint)); //todo:: optimize
            }

            Processor.SafeAppend(suggestedSingleReturnPoint);

            return suggestedSingleReturnPoint;
        }
    }
}