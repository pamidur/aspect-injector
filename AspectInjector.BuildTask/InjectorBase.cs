using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal abstract class InjectorBase
    {
        protected FieldReference GetOrCreateAspectReference(TypeDefinition targetType, TypeDefinition aspectType)
        {
            if (!targetType.IsClass)
                throw new NotSupportedException("Field creation supports only classes.");

            var aspectPropertyName = "__a_" + aspectType.Name;

            var existingField = targetType.Fields.FirstOrDefault(f => f.Name == aspectPropertyName && f.FieldType == aspectType);

            if (existingField != null)
                return existingField;

            var fd = new FieldDefinition(aspectPropertyName, FieldAttributes.Private | FieldAttributes.InitOnly, targetType.Module.Import(aspectType));

            var constructors = targetType.Methods.Where(m => m.IsConstructor && !m.IsStatic);

            foreach (var constructor in constructors)
            {
                var aspectConstructor = targetType.Module.ImportConstructor(aspectType);

                ILProcessor processor = constructor.Body.GetILProcessor();
                Instruction firstInstruction = constructor.Body.Instructions.First();

                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, aspectConstructor));
                processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Stfld, fd));
            }

            targetType.Fields.Add(fd);

            return fd;
        }

        protected void InjectMethodCall(ILProcessor processor, Instruction injectionPoint, MemberReference sourceMember, MethodDefinition method, object[] arguments)
        {
            if (method.Parameters.Count != arguments.Length)
                throw new ArgumentException("Arguments count missmatch", "arguments");

            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Nop));

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
            else
            {
                throw new NotSupportedException("Only FieldRefences supported at the moment");
            }

            for (int i = 0; i < method.Parameters.Count; i++)
                LoadCallArgument(processor, injectionPoint, arguments[i], method.Parameters[i].ParameterType);

            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Callvirt, method));
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
    }
}