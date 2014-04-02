using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Common;

namespace AspectInjector.BuildTask.Processors.AspectProcessors
{
    public class InterfaceProcessor : IAspectProcessor
    {
        private readonly IAspectInjector<InterfaceInjectionContext> _injector;
        public InterfaceProcessor(IAspectInjector<InterfaceInjectionContext> injector)
        {
            _injector = injector;
        }

        public bool CanProcess(TypeDefinition aspectType)
        {
            return aspectType.CustomAttributes.HasAttributeOfType<AdviceInterfaceProxyAttribute>();
        }

        public void Process(AspectContext context)
        {
            var interfaceInjectionDefinitions = from ca in context.AspectType.CustomAttributes
                                                where ca.IsAttributeOfType<AdviceInterfaceProxyAttribute>()
                                                select new { @interface = (TypeReference)ca.ConstructorArguments[0].Value };

            foreach (var interfaceInjectionDefinition in interfaceInjectionDefinitions)
            {
                var interfaceReference = interfaceInjectionDefinition.@interface;
                //todo:: process other InterfaceProxyInjectionAttribute parameters

                var interfaceInjectionContext = new InterfaceInjectionContext
                {
                    AspectContext = context,
                    InterfaceDefinition = interfaceInjectionDefinition.@interface.Resolve()
                };

                FillinInterfaceMembers(interfaceInjectionContext);

                //some validation

                _injector.Inject(interfaceInjectionContext);
            }
        }

        protected virtual void FillinInterfaceMembers(InterfaceInjectionContext context)
        {
            var aspectDefinition = context.AspectContext.AspectType;
            var interfaceDefinition = context.InterfaceDefinition;
            var classDefinition = context.AspectContext.TargetType;

            if (!context.InterfaceDefinition.IsInterface)
                throw new CompilationException(context.InterfaceDefinition.Name + " is not an interface on interface injection definition on acpect " + aspectDefinition.Name, aspectDefinition);

            if (!context.AspectContext.AspectType.ImplementsInterface(context.InterfaceDefinition))
                throw new CompilationException(aspectDefinition.Name + " should implement " + interfaceDefinition.Name, aspectDefinition);

            if (!classDefinition.ImplementsInterface(interfaceDefinition))
            {
                var ifaces = interfaceDefinition.GetInterfacesTree();

                foreach (var iface in ifaces)
                    classDefinition.Interfaces.Add(classDefinition.Module.Import(iface));
            }
            else if (!classDefinition.Interfaces.Contains(interfaceDefinition))
            {
                //In order to behave the same as csc
                classDefinition.Interfaces.Add(interfaceDefinition);
            }

            context.Methods = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Methods)
                .Where(m => !m.IsAddOn && !m.IsRemoveOn && !m.IsSetter && !m.IsGetter)
                .ToArray();

            context.Properties = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Properties)
                .ToArray();

            context.Events = interfaceDefinition.GetInterfaceTreeMemebers(td => td.Events)
                .ToArray();            
        }
    }
}
