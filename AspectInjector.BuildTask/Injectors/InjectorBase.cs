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

                var args = ResolveArgumentsValues(context, context.AspectFactoryArgumentsSources,null).ToArray();
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
            }

            if (arg is VariableDefinition)
            {
                var var = (VariableDefinition)arg;

                if (!expectedType.Resolve().IsTypeOf(var.VariableType))
                    throw new ArgumentException("Argument type mismatch");

                processor.InsertBefore(injectionPoint, processor.CreateOptimized(expectedType.IsByReference ? OpCodes.Ldloca : OpCodes.Ldloc, var.Index));
            }

            if (arg is string)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.String))
                    throw new ArgumentException("Argument type mismatch");

                var str = (string)arg;
                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldstr, str));
            }

            if (arg == Markers.InstanceSelfMarker)
            {
                if (!expectedType.IsTypeOf(module.TypeSystem.Object))
                    throw new ArgumentException("Argument type mismatch");

                processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldarg_0));
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
            }
        }

        protected IEnumerable<object> ResolveArgumentsValues(AspectContext context, List<AdviceArgumentSource> sources, VariableDefinition abortFlagVariable)
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
                        if (abortFlagVariable != null)
                            yield return abortFlagVariable;
                        else
                            throw new ArgumentNullException("abortFlagVariable", "abortFlagVariable should be set for AdviceArgumentSource.AbortFlag");
                        break;

                    case AdviceArgumentSource.CustomData:
                        yield return context.AspectCustomData;
                        break;
                }
            }
        }
    }
}