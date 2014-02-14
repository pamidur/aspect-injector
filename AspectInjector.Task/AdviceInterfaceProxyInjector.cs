using Mono.Cecil;
using System;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class AdviceInterfaceProxyInjector : InjectorBase, IAspectInjector
    {
        public void ProcessModule(ModuleDefinition module)
        {
            var allClasses = module.Types
                .Where(t => t.IsClass && t.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(AspectAttribute))));

            foreach (var @class in allClasses)
            {
                var aspects = from ca in @class.CustomAttributes
                              where ca.IsAttributeOfType(typeof(AspectAttribute))
                              let type = ((TypeReference)ca.ConstructorArguments[0].Value).Resolve()
                              where type.CustomAttributes.Any(tca => tca.IsAttributeOfType(typeof(AdviceInterfaceProxyAttribute)))
                              select new { aspect = ca, type };

                if (!aspects.Any())
                    continue;

                var uniqueAspects = aspects.GroupBy(p => p.type).Select(g => g.First());

                foreach (var aspect in uniqueAspects)
                {
                    //todo:: process other aspect parameters

                    InjectAspectIntoClass(@class, aspect.type);
                }

                //todo:: cleanup aspects
            }
        }

        protected virtual void InjectAspectIntoClass(TypeDefinition classDefinition, TypeDefinition aspectDefinition)
        {
            var interfaceInjectionDefinitions = from ca in aspectDefinition.CustomAttributes
                                                where ca.IsAttributeOfType(typeof(AdviceInterfaceProxyAttribute))
                                                select new { @interface = (TypeReference)ca.ConstructorArguments[0].Value };

            foreach (var interfaceInjectionDefinition in interfaceInjectionDefinitions)
            {
                var interfaceReference = interfaceInjectionDefinition.@interface;
                //todo:: process other InterfaceProxyInjectionAttribute parameters

                if (classDefinition.ImplementsInterface(interfaceReference))
                    throw new InvalidOperationException(classDefinition.Name + " already implements " + interfaceReference.Name);

                InjectInterfaceProxyIntoClass(classDefinition, aspectDefinition, interfaceReference.Resolve());
            }
        }

        protected virtual void InjectInterfaceProxyIntoClass(TypeDefinition classDefinition, TypeDefinition aspectDefinition, TypeDefinition interfaceDefinition)
        {
            if (!interfaceDefinition.IsInterface)
                throw new NotSupportedException(interfaceDefinition.Name + " is not an interface on interface injection definition on acpect " + aspectDefinition.Name);

            if (!aspectDefinition.ImplementsInterface(interfaceDefinition))
                throw new InvalidOperationException(aspectDefinition.Name + " should implement " + interfaceDefinition.Name);

            var ifaces = interfaceDefinition.GetInterfacesTree();

            foreach (var iface in ifaces)
                classDefinition.Interfaces.Add(iface);

            var methods = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Methods);
            var properties = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Properties);
            var events = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Events);

            foreach (var method in methods)
                GetOrCreateMethodProxy(classDefinition, aspectDefinition, method);
        }
    }
}