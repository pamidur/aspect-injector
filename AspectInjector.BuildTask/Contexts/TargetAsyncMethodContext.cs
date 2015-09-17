using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Contexts
{
    internal class TargetAsyncMethodContext : TargetMethodContext
    {
        #region Private Fields

        private static readonly string ContinuationMethodName = "__$a_{0}_continuation";
        private static readonly string ContinuationFieldName = "__$a_{0}_tcs";
        private static readonly string HelperClassName = "__$a_async_continuation";
        private static readonly string HelperClassOriginRefName = "__$a_this_ref";
        private static readonly string HelperClassArgumentsRefName = "__$a_args";
        private static readonly string HelperClassRoutableDataRefName = "__$a_rd";

        private readonly TypeReference _completionResultType;
        private readonly bool _hasResult;
        private readonly TypeReference _taskCompletionTr;
        private readonly TypeReference _taskGenTr;
        private readonly TypeReference _taskTr;

        private FieldDefinition _helperThisRefFiled;
        private PointCut _originalReturnPoint = null;
        private VariableDefinition _resultVar = null;
        private PointCut _returnPoint = null;
        private FieldDefinition _helperArgumentsFiled;
        private FieldDefinition _helperRoutableDataFiled;

        #endregion Private Fields

        #region Public Constructors

        public TargetAsyncMethodContext(MethodDefinition targetMethod) : base(targetMethod)
        {
            _taskTr = targetMethod.Module.Import(typeof(Task));
            _taskGenTr = targetMethod.Module.Import(typeof(Task<>));
            _taskCompletionTr = targetMethod.Module.Import(typeof(TaskCompletionSource<>));

            _hasResult = !TargetMethod.ReturnType.IsTypeOf(_taskTr) && !TargetMethod.ReturnType.IsTypeOf(TargetMethod.Module.TypeSystem.Void);

            _completionResultType = _hasResult ? ((IGenericInstance)TargetMethod.ReturnType).GenericArguments.First() : TargetMethod.Module.TypeSystem.Object;
        }

        #endregion Public Constructors

        #region Public Properties

        public override VariableDefinition MethodResultVariable
        {
            get
            {
                if (TargetMethod.ReturnType.IsTypeOf(typeof(Task)) || TargetMethod.ReturnType.IsTypeOf(typeof(void)))
                    return null;

                if (_resultVar == null)
                    SetupTaskContinuation();

                return _resultVar;
            }
        }

        public override PointCut OriginalReturnPoint
        {
            get
            {
                if (_originalReturnPoint == null)
                    SetupTaskContinuation();

                return _originalReturnPoint;
            }
        }

        public override PointCut ReturnPoint
        {
            get
            {
                if (_returnPoint == null)
                    SetupTaskContinuation();

                return _returnPoint;
            }
        }

        #endregion Public Properties

        #region Private Methods

        private TypeDefinition GetOrCreateHelperClass()
        {
            var helperClass = TargetMethod.DeclaringType.NestedTypes.FirstOrDefault(nc => nc.Name == HelperClassName);

            if (helperClass == null)
            {
                helperClass = new TypeDefinition(null, HelperClassName, TypeAttributes.Sealed | TypeAttributes.NotPublic | TypeAttributes.NestedPrivate, TargetMethod.Module.TypeSystem.Object);
                MarkCompilerGenerated(helperClass);
                TargetMethod.DeclaringType.NestedTypes.Add(helperClass);

                var ctor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    TargetMethod.Module.TypeSystem.Void);

                helperClass.Methods.Add(ctor);

                var ctorProc = ctor.Body.GetILProcessor();
                ctorProc.Append(ctorProc.Create(OpCodes.Ldarg_0));
                ctorProc.Append(ctorProc.Create(OpCodes.Call, TargetMethod.Module.Import(TargetMethod.Module.TypeSystem.Object.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic))));
                ctorProc.Append(ctorProc.Create(OpCodes.Ret));
            }

            _helperThisRefFiled = helperClass.Fields.FirstOrDefault(f => f.Name == HelperClassOriginRefName);
            if (_helperThisRefFiled == null)
            {
                _helperThisRefFiled = new FieldDefinition(HelperClassOriginRefName, FieldAttributes.Public, TargetMethod.DeclaringType);
                helperClass.Fields.Add(_helperThisRefFiled);
            }

            _helperArgumentsFiled = helperClass.Fields.FirstOrDefault(f => f.Name == HelperClassArgumentsRefName);
            if (_helperArgumentsFiled == null)
            {
                _helperArgumentsFiled = new FieldDefinition(HelperClassArgumentsRefName, FieldAttributes.Public, new ArrayType(TargetMethod.Module.TypeSystem.Object));
                helperClass.Fields.Add(_helperArgumentsFiled);
            }

            _helperRoutableDataFiled = helperClass.Fields.FirstOrDefault(f => f.Name == HelperClassRoutableDataRefName);
            if (_helperRoutableDataFiled == null)
            {
                _helperRoutableDataFiled = new FieldDefinition(HelperClassRoutableDataRefName, FieldAttributes.Public, new ArrayType(TargetMethod.Module.TypeSystem.Object));
                helperClass.Fields.Add(_helperRoutableDataFiled);
            }

            return helperClass;
        }

        private void SetupTaskContinuation()
        {
            if (_returnPoint != null || _originalReturnPoint != null)
                throw new InvalidOperationException("Something went wrong");

            var helper = GetOrCreateHelperClass();

            var continuation = new MethodDefinition(
                string.Format(ContinuationMethodName, TargetMethod.Name),
                MethodAttributes.Assembly,
                TargetMethod.Module.TypeSystem.Void);

            helper.Methods.Add(continuation);

            var tcsType = _taskCompletionTr.MakeGenericType(_completionResultType);
            var taskTypedType = continuation.Module.Import(_hasResult ? TargetMethod.ReturnType : _taskTr);

            var taskParameter = new ParameterDefinition(null, ParameterAttributes.None, taskTypedType);
            continuation.Parameters.Add(taskParameter);

            var tcsParameter = new FieldDefinition(string.Format(ContinuationFieldName, TargetMethod.Name), FieldAttributes.Public, tcsType);
            helper.Fields.Add(tcsParameter);

            var proc = continuation.Body.GetILProcessor();
            var ret = proc.Create(OpCodes.Ret);

            proc.Append(ret);

            var pointcut = new AsyncPointCut(_helperThisRefFiled, proc, ret);

            pointcut.LoadParameterOntoStack(taskParameter);
            pointcut.InjectMethodCall(_taskTr.Resolve().Properties.First(p => p.Name == "IsCompleted").GetMethod, new object[] { });

            pointcut.TestValueOnStack(true,
                doIfTrue: pc =>
                {
                    pc.LoadParameterOntoStack(taskParameter);
                    pc.InjectMethodCall(_taskTr.Resolve().Properties.First(p => p.Name == "IsFaulted").GetMethod, new object[] { });
                    pointcut.TestValueOnStack(false,
                       doIfTrue: pct =>
                       {
                           if (_hasResult)
                           {
                               pct.LoadParameterOntoStack(taskParameter);
                               pct.InjectMethodCall(taskTypedType.Resolve().Properties.First(p => p.Name == "Result").GetMethod.MakeGeneric(taskTypedType, _completionResultType), new object[] { });
                               _resultVar = pct.CreateVariableFromStack(_completionResultType);
                           }

                           _returnPoint = pct.InsertBefore(pct.CreateInstruction(OpCodes.Nop));

                           var setresultMethod = tcsType.Resolve().Methods.First(m => m.Name == "SetResult").MakeGeneric(tcsType, _completionResultType);

                           pct.LoadFieldOntoStack(tcsParameter);
                           pct.InjectMethodCall(setresultMethod, new object[] { _resultVar ?? Markers.DefaultMarker });
                       });
                });

            VariableDefinition taskResult = null;

            if (!TargetMethod.ReturnType.IsTypeOf(TargetMethod.Module.TypeSystem.Void))
                taskResult = OriginalEntryPoint.CreateVariable(TargetMethod.ReturnType, "__$a_task");

            var singleReturnPoint = Processor.Create(OpCodes.Nop);
            _originalReturnPoint = new PointCut(Processor, SetupSingleReturnPoint(Processor.Create(OpCodes.Br, singleReturnPoint), taskResult)); //todo:: optimize
            Processor.SafeAppend(singleReturnPoint);

            PointCut continuationPoint = null;

            if (!TargetMethod.ReturnType.IsTypeOf(TargetMethod.Module.TypeSystem.Void))
            {
                continuationPoint = new PointCut(Processor, Processor.SafeAppend(Processor.CreateOptimized(OpCodes.Ldloc, taskResult.Index)));
                Processor.SafeAppend(Processor.Create(OpCodes.Ret));
            }
            else
            {
                continuationPoint = new PointCut(Processor, Processor.SafeAppend(Processor.Create(OpCodes.Ret)));
            }

            continuationPoint.InjectMethodCall(helper.Methods.First(m => m.IsConstructor && !m.IsStatic), new object[] { });
            var helperVar = continuationPoint.CreateVariableFromStack(helper, "__$a_continuation");

            continuationPoint.LoadVariableOntoStack(helperVar);
            continuationPoint.LoadCallArgument(TargetMethod.Parameters.ToArray(), new ArrayType(TargetMethod.Module.TypeSystem.Object));
            continuationPoint.InsertBefore(continuationPoint.CreateInstruction(OpCodes.Stfld, _helperArgumentsFiled));
        }

        #endregion Private Methods
    }
}