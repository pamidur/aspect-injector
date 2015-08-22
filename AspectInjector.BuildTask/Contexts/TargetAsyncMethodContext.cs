using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Contexts
{
    internal class TargetAsyncMethodContext : TargetMethodContext
    {
        private static readonly string HelperClassName = "__$a_async_continuation";
        private static readonly string HelperClassOriginRefName = "__$a_this_ref";
        private static readonly string ContinuationMethodName = "__$a_{0}_continuation";

        private PointCut _originalReturnPoint = null;
        private VariableDefinition _resultVar = null;
        private PointCut _returnPoint = null;
        private FieldDefinition _helperThisRefFiled;

        public TargetAsyncMethodContext(MethodDefinition targetMethod) : base(targetMethod)
        {
        }

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

        private void SetupTaskContinuation()
        {
            var helper = GetOrCreateHelperClass();

            var continuation = new MethodDefinition(
                string.Format(ContinuationMethodName, TargetMethod.Name),
                MethodAttributes.Assembly,
                TargetMethod.Module.TypeSystem.Void);

            helper.Methods.Add(continuation);

            var asyncResultType = TargetMethod.ReturnType.HasGenericParameters ?
                TargetMethod.ReturnType.GenericParameters.First() :
                continuation.Module.TypeSystem.Void;

            var completionResultType = asyncResultType == continuation.Module.TypeSystem.Void ? continuation.Module.TypeSystem.Object : asyncResultType;

            var tcsType = continuation.Module.Import(typeof(TaskCompletionSource<>)).MakeGenericType(completionResultType);

            var taskType = continuation.Module.Import(typeof(Task));
            var taskTypedType = continuation.Module.Import(TargetMethod.ReturnType);

            var tcsParameter = new ParameterDefinition(null, ParameterAttributes.None, tcsType);
            var taskParameter = new ParameterDefinition(null, ParameterAttributes.None, taskTypedType);

            MarkCompilerGenerated(tcsParameter);
            MarkCompilerGenerated(taskParameter);

            continuation.Parameters.Add(taskParameter);
            continuation.Parameters.Add(tcsParameter);

            foreach (var parameter in TargetMethod.Parameters)
                continuation.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, continuation.Module.Import(parameter.ParameterType)));

            var proc = continuation.Body.GetILProcessor();
            var ret = proc.Create(OpCodes.Ret);

            proc.Append(ret);

            var pointcut = new AsyncPointCut(_helperThisRefFiled, proc, ret);

            pointcut.LoadParameterOntoStack(taskParameter);
            pointcut.InjectMethodCall(taskType.Resolve().Properties.First(p => p.Name == "IsCompleted").GetMethod, new object[] { });

            pointcut.TestValueOnStack(true,
                doIfTrue: pc =>
                {
                    pc.LoadParameterOntoStack(taskParameter);
                    pc.InjectMethodCall(taskType.Resolve().Properties.First(p => p.Name == "IsFaulted").GetMethod, new object[] { });
                    pointcut.TestValueOnStack(false,
                       doIfTrue: pct =>
                       {
                           if (asyncResultType != continuation.Module.TypeSystem.Void)
                           {
                               pct.LoadParameterOntoStack(taskParameter);
                               pct.InjectMethodCall(taskTypedType.Resolve().Properties.First(p => p.Name == "Result").GetMethod, new object[] { });
                           }
                           else
                           {
                               pct.InsertBefore(pct.CreateInstruction(OpCodes.Ldnull));
                           }

                           _resultVar = pct.CreateVariableFromStack(completionResultType);
                           _returnPoint = pct.InsertBefore(pct.CreateInstruction(OpCodes.Nop));

                           var setresultMethod = tcsType.Resolve().Methods.First(m => m.Name == "SetResult").MakeGeneric(tcsType, completionResultType);

                           pct.LoadParameterOntoStack(tcsParameter);
                           pct.InjectMethodCall(setresultMethod, new object[] { _resultVar });
                       });
                });
        }

        private TypeDefinition GetOrCreateHelperClass()
        {
            var helperClass = TargetMethod.DeclaringType.NestedTypes.FirstOrDefault(nc => nc.Name == HelperClassName);

            if (helperClass == null)
            {
                helperClass = new TypeDefinition(null, HelperClassName, TypeAttributes.Sealed | TypeAttributes.NotPublic | TypeAttributes.NestedPrivate, TargetMethod.Module.TypeSystem.Object);
                MarkCompilerGenerated(helperClass);
                TargetMethod.DeclaringType.NestedTypes.Add(helperClass);
            }

            _helperThisRefFiled = helperClass.Fields.FirstOrDefault(f => f.Name == HelperClassOriginRefName);

            if (_helperThisRefFiled == null)
            {
                _helperThisRefFiled = new FieldDefinition(HelperClassOriginRefName, FieldAttributes.Public, TargetMethod.DeclaringType);
                helperClass.Fields.Add(_helperThisRefFiled);
            }

            return helperClass;
        }
    }
}