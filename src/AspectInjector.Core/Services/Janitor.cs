using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Services
{
    public class Janitor : IJanitor
    {
        private static readonly byte[] BrokerKeyToken = typeof(Aspect).Assembly.GetName().GetPublicKeyToken();

        private readonly ILogger _log;

        public Janitor(ILogger logger)
        {
            _log = logger;
        }

        public void Cleanup(AssemblyDefinition assembly)
        {
            foreach (var module in assembly.Modules)
                CleanupModule(module);
        }

        private void CleanupModule(ModuleDefinition module)
        {
            RemoveBrokerAttributes(module.CustomAttributes);

            foreach (var type in module.Types)
                RemoveBrokerAttributes(type);

            var reference = module.AssemblyReferences.FirstOrDefault(ar => ar.PublicKeyToken.SequenceEqual(BrokerKeyToken));

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
                if (attr.AttributeType.BelongsToAssembly(BrokerKeyToken))
                    collection.Remove(attr);
        }

        private void CheckTypeReferencesBroker(TypeDefinition type)
        {
            type.Methods.ToList().ForEach(CheckMethodReferencesBroker);

            if ((type.BaseType != null && (type.BaseType.BelongsToAssembly(BrokerKeyToken) || IsGenericInstanceArgumentsReferenceBroker(type.BaseType as IGenericInstance)))
                || type.Fields.Any(f => f.FieldType.BelongsToAssembly(BrokerKeyToken) || IsGenericInstanceArgumentsReferenceBroker(f.FieldType as IGenericInstance))
                || IsGenericParametersReferenceBroker(type))
            {
                _log.LogError(CompilationMessage.From("Types from AspectInjector.Broker can't be referenced", type));
            }

            type.NestedTypes.ToList().ForEach(CheckTypeReferencesBroker);
        }

        private void CheckMethodReferencesBroker(MethodDefinition method)
        {
            if (method.Parameters.Any(p => p.ParameterType.BelongsToAssembly(BrokerKeyToken) || IsGenericInstanceArgumentsReferenceBroker(p.ParameterType as IGenericInstance))
                || (method.Body != null && IsMethodBodyReferencesBroker(method.Body))
                || method.ReturnType.BelongsToAssembly(BrokerKeyToken)
                || IsGenericParametersReferenceBroker(method)
                || IsGenericInstanceArgumentsReferenceBroker(method.ReturnType as IGenericInstance))
            {
                _log.LogError(CompilationMessage.From("Types from AspectInjector.Broker can't be referenced", method));
            }
        }

        private bool IsGenericParametersReferenceBroker(IGenericParameterProvider genericParameters)
        {
            return genericParameters.GenericParameters.Any(p =>
                p.Constraints.Any(c => c.BelongsToAssembly(BrokerKeyToken)
                    || IsGenericInstanceArgumentsReferenceBroker(c as IGenericInstance)));
        }

        private bool IsGenericInstanceArgumentsReferenceBroker(IGenericInstance genericInstance)
        {
            if (genericInstance != null)
            {
                return genericInstance.GenericArguments.Any(a => a.BelongsToAssembly(BrokerKeyToken) ||
                    IsGenericInstanceArgumentsReferenceBroker(a as IGenericInstance));
            }

            return false;
        }

        private bool IsMethodBodyReferencesBroker(MethodBody methodBody)
        {
            return methodBody.Variables.Any(v => v.VariableType.BelongsToAssembly(BrokerKeyToken))
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
                    return type.BelongsToAssembly(BrokerKeyToken) ||
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
                    return method.DeclaringType.BelongsToAssembly(BrokerKeyToken) ||
                        IsGenericInstanceArgumentsReferenceBroker(method as GenericInstanceMethod);
                }
            }

            return false;
        }
    }
}