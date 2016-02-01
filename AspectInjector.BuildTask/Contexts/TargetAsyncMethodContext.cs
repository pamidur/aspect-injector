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

        private static readonly string ContinuationFieldName = "__$a_{0}_tcs";
        private static readonly string ContinuationMethodName = "__$a_{0}_continuation";
        private static readonly string HelperClassArgumentsRefName = "__$a_arguments";
        private static readonly string HelperClassName = "__$a_async_continuation";
        private static readonly string HelperClassOriginRefName = "__$a_this_ref";

        private readonly bool _hasResult;
        private readonly bool _isVoid;
        private readonly TypeReference _completionResultType;

        private FieldDefinition _helperArgumentsFiled;
        private FieldDefinition _helperThisRefFiled;
        private PointCut _originalReturnPoint = null;
        private VariableDefinition _resultVar = null;
        private PointCut _returnPoint = null;

        #endregion Private Fields

        #region Public Constructors

        public TargetAsyncMethodContext(MethodDefinition targetMethod, ModuleContext mc) : base(targetMethod, mc)
        {
            _isVoid = TargetMethod.ReturnType.IsTypeOf(TargetMethod.Module.TypeSystem.Void);

            _hasResult = !TargetMethod.ReturnType.IsTypeOf(ModuleContext.TypeSystem.Task) && !_isVoid;

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

            var tcsType = ModuleContext.TypeSystem.TaskCompletionGeneric.MakeGenericType(_completionResultType);
            var taskTypedType = continuation.Module.Import(_hasResult ? TargetMethod.ReturnType : ModuleContext.TypeSystem.Task);

            var taskParameter = new ParameterDefinition(null, ParameterAttributes.None, taskTypedType);
            continuation.Parameters.Add(taskParameter);

            var tcsField = new FieldDefinition(string.Format(ContinuationFieldName, TargetMethod.Name), FieldAttributes.Public, tcsType);
            helper.Fields.Add(tcsField);

            var proc = continuation.Body.GetILProcessor();
            var ret = proc.Create(OpCodes.Ret);

            proc.Append(ret);

            var pointcut = new PointCut(proc, ret);

            pointcut.LoadParameterOntoStack(taskParameter);
            pointcut.InjectMethodCall(ModuleContext.TypeSystem.Task.Resolve().Properties.First(p => p.Name == "IsCompleted").GetMethod, new object[] { });

            pointcut.TestValueOnStack(true,
                doIfTrue: pc =>
                {
                    pc.LoadParameterOntoStack(taskParameter);
                    pc.InjectMethodCall(ModuleContext.TypeSystem.Task.Resolve().Properties.First(p => p.Name == "IsFaulted").GetMethod, new object[] { });
                    pc.TestValueOnStack(false,
                       doIfTrue: pct =>
                       {
                           if (_hasResult)
                           {
                               pct.LoadParameterOntoStack(taskParameter);
                               pct.InjectMethodCall(taskTypedType.Resolve().Properties.First(p => p.Name == "Result").GetMethod.MakeGeneric(taskTypedType), new object[] { });
                               _resultVar = pct.CreateVariableFromStack(_completionResultType);
                           }

                           var syncReturnPc = pct.InsertBefore(pct.CreateInstruction(OpCodes.Nop));
                           _returnPoint = new AsyncPointCut(_helperThisRefFiled, _helperArgumentsFiled, proc, syncReturnPc.InjectionPoint);

                           var setresultMethod = tcsType.Resolve().Methods.First(m => m.Name == "SetResult").MakeGeneric(tcsType);

                           pct.LoadSelfOntoStack();
                           pct.LoadFieldOntoStack(tcsField);
                           pct.InjectMethodCall(setresultMethod, new object[] { _resultVar ?? Markers.DefaultMarker });
                       });
                },
                doIfFalse: pc =>
                {
                    var setresultMethod = tcsType.Resolve().Methods.First(m => m.Name == "SetResult").MakeGeneric(tcsType);

                    pc.LoadSelfOntoStack();
                    pc.LoadFieldOntoStack(tcsField);
                    pc.InjectMethodCall(setresultMethod, new object[] { Markers.DefaultMarker });
                });

            VariableDefinition taskResult = null;

            if (_isVoid)
            {
                taskResult = OriginalEntryPoint.CreateVariable(ModuleContext.TypeSystem.Task);
            }
            else
            {
                taskResult = OriginalEntryPoint.CreateVariable(TargetMethod.ReturnType);
            }

            var singleReturnPoint = Processor.Create(OpCodes.Nop);
            _originalReturnPoint = new PointCut(Processor, SetupSingleReturnPoint(Processor.Create(OpCodes.Br, singleReturnPoint), taskResult)); //todo:: optimize

            if (_isVoid)
                AsyncVoidRewriter.Rewrite(_originalReturnPoint, TargetMethod, taskResult);

            Processor.SafeAppend(singleReturnPoint);

            PointCut continuationPoint = null;

            if (!_isVoid)
            {
                continuationPoint = new PointCut(Processor, Processor.SafeAppend(Processor.CreateOptimized(OpCodes.Ldloc, taskResult.Index)));
                Processor.SafeAppend(Processor.Create(OpCodes.Ret));
            }
            else
            {
                continuationPoint = new PointCut(Processor, Processor.SafeAppend(Processor.Create(OpCodes.Ret)));
            }

            // var tcs = new TaskContinuationSource<TResult>();
            var tcsctor = tcsType.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic).MakeGeneric(tcsType);
            continuationPoint.InjectMethodCall(tcsctor, new object[] { });
            var tcsVar = continuationPoint.CreateVariableFromStack(tcsType);

            // var helper = new Helper();
            continuationPoint.InjectMethodCall(helper.Methods.First(m => m.IsConstructor && !m.IsStatic), new object[] { });
            var helperVar = continuationPoint.CreateVariableFromStack(helper);

            // var args = new object[] { param1, param2 ... };
            continuationPoint.LoadCallArgument(TargetMethod.Parameters.ToArray(), new ArrayType(TargetMethod.Module.TypeSystem.Object));
            var argsvar = continuationPoint.CreateVariableFromStack(new ArrayType(TargetMethod.Module.TypeSystem.Object));

            //helper.this_ref = this
            continuationPoint.LoadVariableOntoStack(helperVar);
            continuationPoint.LoadSelfOntoStack();
            continuationPoint.SetFieldFromStack(_helperThisRefFiled);

            // helper.args = args
            continuationPoint.LoadVariableOntoStack(helperVar);
            continuationPoint.LoadVariableOntoStack(argsvar);
            continuationPoint.SetFieldFromStack(_helperArgumentsFiled);

            // helper.continuationSource = tcs
            continuationPoint.LoadVariableOntoStack(helperVar);
            continuationPoint.LoadVariableOntoStack(tcsVar);
            continuationPoint.SetFieldFromStack(tcsField);

            // task.ContinueWith(new Action<TResult>(helper.Continuation))
            continuationPoint.LoadVariableOntoStack(helperVar);
            continuationPoint.InsertBefore(continuationPoint.CreateInstruction(OpCodes.Ldftn, continuation));

            var actionTr = continuation.Module.Import(ModuleContext.TypeSystem.ActionGeneric.MakeGenericType(taskTypedType));

            var contActionCtor = continuation.Module.Import(actionTr.Resolve().Methods.First(m => m.IsConstructor && !m.IsStatic))
                .MakeGeneric(actionTr);
            continuationPoint.InsertBefore(continuationPoint.CreateInstruction(OpCodes.Newobj, (MethodReference)continuationPoint.CreateMemberReference(contActionCtor)));

            var actionVar = continuationPoint.CreateVariableFromStack(actionTr);

            MethodReference contWithMethod = continuation.Module.Import(taskTypedType.Resolve().Methods.First(m => m.Name == "ContinueWith" && m.Parameters.Count == 1));
            if (_hasResult)
                contWithMethod = contWithMethod.MakeGeneric(taskTypedType);

            continuationPoint.LoadVariableOntoStack(taskResult);
            continuationPoint.InjectMethodCall(contWithMethod, new object[] { actionVar });
            continuationPoint.InsertBefore(continuationPoint.CreateInstruction(OpCodes.Pop));

            // task = tcs.Task
            if (!_isVoid)
            {
                var getTask = continuation.Module.Import(tcsType.Resolve().Properties.First(p => p.Name == "Task").GetMethod)
                    .MakeGeneric(tcsType);

                continuationPoint.LoadVariableOntoStack(tcsVar);
                continuationPoint.InjectMethodCall(getTask, new object[] { });
                continuationPoint.SetVariableFromStack(taskResult);
            }
        }

        #endregion Private Methods
    }
}