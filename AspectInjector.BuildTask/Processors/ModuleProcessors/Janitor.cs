using System;
using System.Linq;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class Janitor : IModuleProcessor
    {
        private static readonly string _brokerAssemblyPublicKeyToken = "a29e12442a3d3609";

        public void ProcessModule(Mono.Cecil.ModuleDefinition module)
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
        }

        public void RemoveBrokerAttributes(Collection<CustomAttribute> collection)
        {
            foreach (var attr in collection.ToList())
                if (attr.AttributeType.BelongsToAssembly(_brokerAssemblyPublicKeyToken))
                    collection.Remove(attr);
        }
    }
}