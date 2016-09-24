using System.Linq;
using AspectInjector.BuildTask.Extensions;
using AspectInjector.BuildTask.Models;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class FoundAspectDefinition
    {
        public AspectDefinition AspectDefinition { get; }
        public FoundOn FoundOn { get; }

        public TypeDefinition FoundOnInterface { get; }
        public MethodDefinition FoundOnMember { get; }

        public FoundAspectDefinition(AspectDefinition aspectDefinition, FoundOn foundOn, MethodDefinition foundOnMember)
            : this(aspectDefinition, foundOn)
        {
            FoundOnMember = foundOnMember;
        }

        public FoundAspectDefinition(AspectDefinition aspectDefinition, FoundOn foundOn, TypeDefinition foundOnInterface)
            : this(aspectDefinition, foundOn)
        {
            FoundOnInterface = foundOnInterface;
        }

        public FoundAspectDefinition(AspectDefinition aspectDefinition, FoundOn foundOn)
        {
            AspectDefinition = aspectDefinition;
            FoundOn = foundOn;
        }

        public bool AppliesTo(MethodDefinition targetMethod)
        {
            switch (FoundOn)
            {
                case FoundOn.Assembly:
                    return true;

                case FoundOn.BaseType:
                    return true;

                case FoundOn.BaseTypeMember:
                    return targetMethod.GetBaseMethod() == FoundOnMember;

                case FoundOn.Interface:
                    return FoundOnInterface.Methods.Any(targetMethod.IsInterfaceImplementation);

                case FoundOn.InterfaceMember:
                    return targetMethod.IsInterfaceImplementation(FoundOnMember);

                case FoundOn.Type:
                    return true;

                default:
                    return false;
            }
        }
    }
}