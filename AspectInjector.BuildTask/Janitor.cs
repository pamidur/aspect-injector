using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class Janitor : IModuleProcessor
    {
        private static readonly string brokerAssemblyPublicKeyToken = "a29e12442a3d3609";

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

            var reference = module.AssemblyReferences.FirstOrDefault(ar => BitConverter.ToString(ar.PublicKeyToken).Replace("-", "").ToLowerInvariant() == brokerAssemblyPublicKeyToken.ToLowerInvariant());
            if (reference != null)
                module.AssemblyReferences.Remove(reference);
        }

        public void RemoveBrokerAttributes(Collection<CustomAttribute> collection)
        {
            foreach (var attr in collection.ToList())
                if (attr.AttributeType.BelongsToAssembly(brokerAssemblyPublicKeyToken))
                    collection.Remove(attr);
        }
    }
}