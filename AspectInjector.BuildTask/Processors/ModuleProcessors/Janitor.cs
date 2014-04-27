using System;
using System.Linq;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class Janitor : IModuleProcessor
    {
        private static readonly string _brokerAssemblyPublicKeyToken = "a29e12442a3d3609";

        public void ProcessModule(ModuleDefinition module)
        {
            RemoveBrokerAttributes(module.CustomAttributes);

            foreach (var type in module.Types)
            {
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

            var reference = module.AssemblyReferences.FirstOrDefault(ar => BitConverter.ToString(ar.PublicKeyToken).Replace("-", "").ToLowerInvariant() == _brokerAssemblyPublicKeyToken.ToLowerInvariant());
            if (reference != null)
                module.AssemblyReferences.Remove(reference);

            module.Types.ToList().ForEach(CheckTypeReferencesBroker);
        }

        private void RemoveBrokerAttributes(Collection<CustomAttribute> collection)
        {
            foreach (var attr in collection.ToList())
                if (attr.AttributeType.BelongsToAssembly(_brokerAssemblyPublicKeyToken))
                    collection.Remove(attr);
        }

        private void CheckTypeReferencesBroker(TypeDefinition type)
        {
            type.Methods.ToList().ForEach(CheckMethodReferencesBroker);

            if (type.BaseType != null && type.BaseType.BelongsToAssembly(_brokerAssemblyPublicKeyToken) ||
                type.Fields.Any(f => f.FieldType.BelongsToAssembly(_brokerAssemblyPublicKeyToken)))
            {
                throw new CompilationException("Types from AspectInjector.Broker can't be referenced", type);
            }

            CheckGenericParametersReferenceBroker(type);
        }

        private void CheckGenericParametersReferenceBroker(TypeDefinition type)
        {
            if (type.GenericParameters.Any(p => p.BelongsToAssembly(_brokerAssemblyPublicKeyToken)))
            {
                throw new CompilationException("Types from AspectInjector.Broker can't be referenced", type);
            }
            type.GenericParameters.ToList().ForEach(p => CheckGenericParametersReferenceBroker(p.Resolve()));
        }

        private void CheckMethodReferencesBroker(MethodDefinition method)
        {
            if (method.Parameters.Any(p => p.ParameterType.BelongsToAssembly(_brokerAssemblyPublicKeyToken))
                || (method.Body != null && method.Body.Variables.Any(v => v.VariableType.BelongsToAssembly(_brokerAssemblyPublicKeyToken)))
                || method.ReturnType.BelongsToAssembly(_brokerAssemblyPublicKeyToken))
            {
                throw new CompilationException("Types from AspectInjector.Broker can't be referenced", method);
            }
        }
    }
}