using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Contexts;
using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Linq;

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

        public void Process(AspectInjectionInfo context)
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
            var classDefinition = context.AspectContext.TargetTypeContext;

            if (!context.InterfaceDefinition.IsInterface)
                throw new CompilationException(context.InterfaceDefinition.Name + " is not an interface on interface injection definition on acpect " + aspectDefinition.Name, aspectDefinition);

            if (!context.AspectContext.AspectType.ImplementsInterface(context.InterfaceDefinition))
                throw new CompilationException(aspectDefinition.Name + " should implement " + interfaceDefinition.Name, aspectDefinition);

            if (!classDefinition.TypeDefinition.ImplementsInterface(interfaceDefinition))
            {
                var ifaces = interfaceDefinition.GetInterfacesTree();

                foreach (var iface in ifaces)
                    classDefinition.TypeDefinition.Interfaces.Add(classDefinition.TypeDefinition.Module.Import(iface));
            }
            else if (!classDefinition.TypeDefinition.Interfaces.Contains(interfaceDefinition))
            {
                //In order to behave the same as csc
                classDefinition.TypeDefinition.Interfaces.Add(interfaceDefinition);
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