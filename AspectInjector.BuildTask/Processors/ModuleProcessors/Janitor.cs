using System;
using System.Linq;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class Janitor : IModuleProcessor
    {
        private static readonly string BrokerAssemblyPublicKeyToken = "a29e12442a3d3609";

        public void ProcessModule(ModuleDefinition module)
        {
            RemoveBrokerAttributes(module.CustomAttributes);

            foreach (var type in module.Types)
                RemoveBrokerAttributes(type);

            var reference = module.AssemblyReferences.FirstOrDefault(
                ar => BitConverter.ToString(ar.PublicKeyToken)
                    .Replace("-", string.Empty).ToLowerInvariant() == BrokerAssemblyPublicKeyToken.ToLowerInvariant());

            if (reference != null)
                module.AssemblyReferences.Remove(reference);

            module.Types.ToList().ForEach(CheckTypeReferencesBroker);
        }

        private void RemoveBrokerAttributes(TypeDefinition type)
        {
            foreach (var nestedType in type.NestedTypes)
                RemoveBrokerAttributes(nestedType);

            RemoveBrokerAttributes(type.CustomAttributes);

            foreach (var method in type.Methods)
            {
                RemoveBrokerAttributes(method.CustomAttributes);
                RemoveBrokerAttributes(method.MethodReturnType.CustomAttributes);

                foreach (var parameter in method.Parameters)
                    RemoveBrokerAttributes(parameter.CustomAttributes);
            }

            foreach (var property in type.Properties)
                RemoveBrokerAttributes(property.CustomAttributes);

            foreach (var @event in type.Events)
                RemoveBrokerAttributes(@event.CustomAttributes);
        }

        private void RemoveBrokerAttributes(Collection<CustomAttribute> collection)
        {
            foreach (var attr in collection.ToList())
                if (attr.AttributeType.BelongsToAssembly(BrokerAssemblyPublicKeyToken))
                    collection.Remove(attr);
        }

        private void CheckTypeReferencesBroker(TypeDefinition type)
        {
            type.Methods.ToList().ForEach(CheckMethodReferencesBroker);

            if ((type.BaseType != null && (type.BaseType.BelongsToAssembly(BrokerAssemblyPublicKeyToken) || IsGenericInstanceArgumentsReferenceBroker(type.BaseType as IGenericInstance)))
                || type.Fields.Any(f => f.FieldType.BelongsToAssembly(BrokerAssemblyPublicKeyToken) || IsGenericInstanceArgumentsReferenceBroker(f.FieldType as IGenericInstance))
                || IsGenericParametersReferenceBroker(type))
            {
                throw new CompilationException("Types from AspectInjector.Broker can't be referenced", type);
            }

            type.NestedTypes.ToList().ForEach(CheckTypeReferencesBroker);
        }

        private void CheckMethodReferencesBroker(MethodDefinition method)
        {
            if (method.Parameters.Any(p => p.ParameterType.BelongsToAssembly(BrokerAssemblyPublicKeyToken) || IsGenericInstanceArgumentsReferenceBroker(p.ParameterType as IGenericInstance))
                || (method.Body != null && IsMethodBodyReferencesBroker(method.Body))
                || method.ReturnType.BelongsToAssembly(BrokerAssemblyPublicKeyToken)
                || IsGenericParametersReferenceBroker(method)
                || IsGenericInstanceArgumentsReferenceBroker(method.ReturnType as IGenericInstance))
            {
                throw new CompilationException("Types from AspectInjector.Broker can't be referenced", method);
            }
        }

        private bool IsGenericParametersReferenceBroker(IGenericParameterProvider genericParameters)
        {
            return genericParameters.GenericParameters.Any(p =>
                p.Constraints.Any(c => c.BelongsToAssembly(BrokerAssemblyPublicKeyToken)
                    || IsGenericInstanceArgumentsReferenceBroker(c as IGenericInstance)));
        }

        private bool IsGenericInstanceArgumentsReferenceBroker(IGenericInstance genericInstance)
        {
            if (genericInstance != null)
            {
                return genericInstance.GenericArguments.Any(a => a.BelongsToAssembly(BrokerAssemblyPublicKeyToken) ||
                    IsGenericInstanceArgumentsReferenceBroker(a as IGenericInstance));
            }

            return false;
        }

        private bool IsMethodBodyReferencesBroker(MethodBody methodBody)
        {
            return methodBody.Variables.Any(v => v.VariableType.BelongsToAssembly(BrokerAssemblyPublicKeyToken))
                || methodBody.Instructions.Any(IsInstructionReferencesBroker);
        }

        private bool IsInstructionReferencesBroker(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Ldtoken ||
                instruction.OpCode == OpCodes.Isinst ||
                instruction.OpCode == OpCodes.Castclass ||
                instruction.OpCode == OpCodes.Newarr)
            {
                TypeReference type = instruction.Operand as TypeReference;
                if (type != null)
                {
                    return type.BelongsToAssembly(BrokerAssemblyPublicKeyToken) ||
                        IsGenericInstanceArgumentsReferenceBroker(type as GenericInstanceType);
                }
            }

            if (instruction.OpCode == OpCodes.Ldtoken ||
                instruction.OpCode == OpCodes.Call ||
                instruction.OpCode == OpCodes.Callvirt ||
                instruction.OpCode == OpCodes.Newobj)
            {
                MethodReference method = instruction.Operand as MethodReference;
                if (method != null)
                {
                    return method.DeclaringType.BelongsToAssembly(BrokerAssemblyPublicKeyToken) ||
                        IsGenericInstanceArgumentsReferenceBroker(method as GenericInstanceMethod);
                }
            }

            return false;
        }
    }
}