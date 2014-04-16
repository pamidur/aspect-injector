using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Injectors
{
    internal abstract class InjectorBase
    {
        protected FieldReference GetOrCreateAspectReference(AspectContext context)
        {
            if (!context.TargetType.IsClass)
                throw new NotSupportedException("Field creation supports only classes.");

            var aspectPropertyName = "__a$_" + context.AspectType.Name;

            var existingField = context.TargetType.Fields.FirstOrDefault(f => f.Name == aspectPropertyName && f.FieldType == context.AspectType);

            if (existingField != null)
                return existingField;

            var fd = new FieldDefinition(aspectPropertyName, FieldAttributes.Private | FieldAttributes.InitOnly, context.TargetType.Module.Import(context.AspectType));

            var constructors = context.TargetType.Methods.Where(m => m.IsConstructor && !m.IsStatic);

            foreach (var constructor in constructors)
            {
                ILProcessor processor = constructor.Body.GetILProcessor();
                Instruction firstInstruction = constructor.Body.Instructions.Skip(2).First();

                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));

                var args = ResolveArgumentsValues(context, context.AspectFactoryArgumentsSources, InjectionPoints.Before).ToArray();
                InjectMethodCall(processor, firstInstruction, null, context.AspectFactory, args);

                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Stfld, fd));
            }

            context.TargetType.Fields.Add(fd);

            return fd;
        }

        protected void InjectMethodCall(ILProcessor processor, Instruction injectionPoint, MemberReference sourceMember, MethodDefinition method, object[] arguments)
        {
            if (method.Parameters.Count != arguments.Length)
                throw new ArgumentException("Arguments count mismatch", "arguments");

            //processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Nop));

            var type = processor.Body.Method.DeclaringType;

            if (sourceMember is FieldReference)
            {
                var fr = (FieldReference)sourceMember;

                if (fr.Resolve().IsStatic)
                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldsfld, fr));
                else
                {
                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldfld, fr));
                }
            }
            else if (sourceMember == null)
            {
                //Nothig should be loaded into stack
            }
            else
            {
                throw new NotSupportedException("Only FieldRefences and nulls supported at the moment");
            }

            for (int i = 0; i < method.Parameters.Count; i++)
                LoadCallArgument(processor, injectionPoint, arguments[i], method.Parameters[i].ParameterType);

            OpCode code;

            if (method.IsConstructor)
                code = OpCodes.Newobj;
            else if (method.IsVirtual)
                code = OpCodes.Callvirt;
            else
                code = OpCodes.Call;

            processor.InsertBefore(injectionPoint, processor.Create(code, processor.Body.Method.Module.Import(method)));
        }

        protected void LoadCallArgument(ILProcessor processor, Instruction injectionPoint, object arg, TypeReference expectedType)
        {
            var module = processor.Body.Method.Module;

            if (arg is ParameterDefinition)
            {
                var parameter = (ParameterDefinition)arg;

                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldarg, parameter.Index + 1));

                if (parameter.ParameterType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Box, module.Import(parameter.ParameterType)));

                return;
            }

            if (arg is VariableDefinition)
            {
                var var = (VariableDefinition)arg;

                if (!expectedType.Resolve().IsTypeOf(var.VariableType))
                    throw new ArgumentException("Argument type mismatch");//todo:: fix for boxing, return value might be by ref

                processor.InsertBefore(injectionPoint, processor.CreateOptimized(expectedType.IsByReference ? OpCodes.Ldloca : OpCodes.Ldloc, var.Index));

                return;
            }

            if (arg is string)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.String))
                    throw new ArgumentException("Argument type mismatch");

                var str = (string)arg;
                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldstr, str));

                return;
            }

            if (arg is Enum)
            {
                //todo:: type check
                var i = (int)arg;
                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldc_I4, i));

                return;
            }

            if (arg == Markers.InstanceSelfMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object))
                    throw new ArgumentException("Argument type mismatch");

                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldarg_0));

                return;
            }

            if (arg == Markers.DefaultMarker)
            {
                if (!expectedType.IsTypeOf(expectedType.Module.TypeSystem.Void))
                {
                    if (expectedType.IsValueType)
                        processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I4_0));
                    else
                        processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldnull));
                }

                return;
            }

            if (arg is Array)
            {
                if (!expectedType.IsTypeOf(new ArrayType(module.TypeSystem.Object)))
                    throw new ArgumentException("Argument type mismatch");

                var parameters = (object[])arg;

                var objectDef = module.Import(module.TypeSystem.Object.Resolve());

                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldc_I4, parameters.Length));
                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Newarr, objectDef));

                if (parameters.Length > 0)
                {
                    processor.Body.InitLocals = true;

                    var paramsArrayVar = new VariableDefinition(new ArrayType(objectDef));
                    processor.Body.Variables.Add(paramsArrayVar);

                    processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Stloc, paramsArrayVar.Index));

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, paramsArrayVar.Index));
                        processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldc_I4, i));

                        LoadCallArgument(processor, injectionPoint, parameters[i], module.TypeSystem.Object);

                        processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Stelem_Ref));
                    }

                    processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldloc, paramsArrayVar.Index));
                }

                return;
            }

            throw new NotSupportedException("Argument type of " + arg.GetType().ToString() + " is not supported");
        }

        protected IEnumerable<object> ResolveArgumentsValues(
            AspectContext context,
            List<AdviceArgumentSource> sources,
            InjectionPoints injectionPointFired,
            VariableDefinition abortFlagVariable = null,
            VariableDefinition exceptionVariable = null,
            VariableDefinition returnObjectVariable = null
             )
        {
            foreach (var argumentSource in sources)
            {
                switch (argumentSource)
                {
                    case AdviceArgumentSource.Instance:
                        yield return Markers.InstanceSelfMarker;
                        break;

                    case AdviceArgumentSource.TargetArguments:
                        yield return context.TargetMethodContext.TargetMethod.Parameters.ToArray();
                        break;

                    case AdviceArgumentSource.TargetName:
                        yield return context.TargetName;
                        break;

                    case AdviceArgumentSource.InjectionPointFired:
                        yield return injectionPointFired;
                        break;

                    case AdviceArgumentSource.AbortFlag:
                        yield return abortFlagVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.Exception:
                        yield return exceptionVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.ReturningValue:
                        yield return returnObjectVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.CustomData:
                        yield return context.AspectCustomData;
                        break;

                    default: throw new NotSupportedException(argumentSource.ToString() + " is not supported (yet?)");
                }
            }
        }
    }
}