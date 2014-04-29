using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
                Instruction firstInstruction = constructor.FindBaseClassCtorCall();

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
                {
                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldsfld, fr));
                }
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
            }
            else if (arg is VariableDefinition)
            {
                var var = (VariableDefinition)arg;

                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.Resolve().IsTypeOf(var.VariableType))
                    throw new ArgumentException("Argument type mismatch");

                processor.InsertBefore(injectionPoint, processor.CreateOptimized(expectedType.IsByReference ? OpCodes.Ldloca : OpCodes.Ldloc, var.Index));

                if (var.VariableType.IsValueType && expectedType.IsTypeOf(module.TypeSystem.Object))
                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Box, module.Import(var.VariableType)));
            }
            else if (arg is string)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(module.TypeSystem.String))
                    throw new ArgumentException("Argument type mismatch");

                var str = (string)arg;
                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldstr, str));
            }
            else if (arg is CustomAttributeArgument)
            {
                var caa = (CustomAttributeArgument)arg;

                if (caa.Type.IsArray)
                {
                    if (!expectedType.IsTypeOf(module.TypeSystem.Object) &&
                        !expectedType.IsTypeOf(new ArrayType(module.TypeSystem.Object))
                        )
                        throw new ArgumentException("Argument type mismatch");

                    LoadArray(injectionPoint, processor, caa.Value, caa.Type.GetElementType(), expectedType);
                }
                else if (caa.Value is CustomAttributeArgument || caa.Type.IsTypeOf(module.TypeSystem.String))
                {
                    LoadCallArgument(processor, injectionPoint, caa.Value, expectedType);
                }
                else
                {
                    LoadValueTypedArgument(injectionPoint, processor, caa.Value, caa.Type, expectedType);
                }
            }
            else if (arg == Markers.InstanceSelfMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object))
                    throw new ArgumentException("Argument type mismatch");

                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldarg_0));
            }
            else if (arg == Markers.DefaultMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Void))
                {
                    if (expectedType.IsValueType)
                        processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I4_0));
                    else
                        processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldnull));
                }
            }
            else if (arg is TypeReference)
            {
                var typeOfType = module.TypeSystem.ResolveType(typeof(Type));

                if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(typeOfType))
                    throw new ArgumentException("Argument type mismatch");

                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldtoken, (TypeReference)arg));
                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Call, module.Import(typeOfType.Resolve().Methods.First(m => m.Name == "GetTypeFromHandle"))));
            }
            else if (arg.GetType().IsValueType)
            {
                var type = module.TypeSystem.ResolveType(arg.GetType());
                LoadValueTypedArgument(injectionPoint, processor, arg, type, expectedType);
            }
            else if (arg is Array)
            {
                var elementType = arg.GetType().GetElementType();

                if (elementType == typeof(ParameterDefinition))
                    elementType = typeof(object);

                LoadArray(injectionPoint, processor, arg, module.TypeSystem.ResolveType(elementType), expectedType);
            }
            else
            {
                throw new NotSupportedException("Argument type of " + arg.GetType().ToString() + " is not supported");
            }
        }

        private void LoadArray(Instruction injectionPoint, ILProcessor processor, object args, TypeReference targetElementType, TypeReference expectedType)
        {
            var module = processor.Body.Method.Module;

            if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(new ArrayType(module.TypeSystem.Object)))
                throw new ArgumentException("Argument type mismatch");

            var parameters = ((Array)args).Cast<object>().ToArray();

            var elementType = module.Import(targetElementType.Resolve());

            processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldc_I4, parameters.Length));
            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Newarr, elementType));

            if (parameters.Length > 0)
            {
                processor.Body.InitLocals = true;

                var paramsArrayVar = new VariableDefinition(new ArrayType(elementType));
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
        }

        private void LoadValueTypedArgument(Instruction injectionPoint, ILProcessor processor, object arg, TypeReference type, TypeReference expectedType)
        {
            if (!arg.GetType().IsValueType)
                throw new NotSupportedException("Only value types are supported.");

            var module = processor.Body.Method.Module;

            if (!expectedType.IsTypeOf(module.TypeSystem.Object) && !expectedType.IsTypeOf(type))
                throw new ArgumentException("Argument type mismatch");

            if (arg is long || arg is ulong || arg is double)
            {
                var rawdata = GetRawValueType(arg, 8);
                var val = BitConverter.ToInt64(rawdata, 0);

                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldc_I8, val));
            }
            else
            {
                var rawdata = GetRawValueType(arg, 4);
                var val = BitConverter.ToInt32(rawdata, 0);

                processor.InsertBefore(injectionPoint, processor.CreateOptimized(OpCodes.Ldc_I4, val));
            }

            if (expectedType.IsTypeOf(module.TypeSystem.Object))
                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Box, module.Import(type)));
        }

        private byte[] GetRawValueType(object value, int @base = 0)
        {
            byte[] rawdata = new byte[@base == 0 ? Marshal.SizeOf(value) : @base];

            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return rawdata;
        }

        protected IEnumerable<object> ResolveArgumentsValues(
            AspectContext context,
            List<AdviceArgumentSource> sources,
            InjectionPoints injectionPointFired,
            VariableDefinition abortFlagVariable = null,
            VariableDefinition exceptionVariable = null,
            VariableDefinition returnObjectVariable = null)
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

                    case AdviceArgumentSource.AbortFlag:
                        yield return abortFlagVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.TargetException:
                        yield return exceptionVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.TargetReturnValue:
                        yield return returnObjectVariable ?? Markers.DefaultMarker;
                        break;

                    case AdviceArgumentSource.CustomData:
                        yield return context.AspectCustomData ?? Markers.DefaultMarker;
                        break;

                    default:
                        throw new NotSupportedException(argumentSource.ToString() + " is not supported (yet?)");
                }
            }
        }
    }
}