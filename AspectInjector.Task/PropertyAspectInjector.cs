using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask
{
    public class PropertyAspectInjector : MethodAspectInjector, IAspectInjector
    {
        public override void ProcessModule(Mono.Cecil.ModuleDefinition module)
        {
            //todo:: make it be able to get all properties of marked class

            //Get properties marked for aspect
            var allProperties = module.Types
                .Where(t => t.IsClass)
                .SelectMany(t => t.Properties).Where(p => p.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(AspectAttribute))));

            ProcessDefinitions(allProperties);
        }

        protected override IEnumerable<MethodDefinition> GetInjectableAspectMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(PropertyInjectionAttribute))));
        }

        protected override IEnumerable<Instruction> GetInjectionPoints(MethodDefinition methodToInject, MethodDefinition injectionTarget)
        {
            var propertyInjectorAttribute = methodToInject.CustomAttributes.First(ca => ca.IsAttributeOfType(typeof(PropertyInjectionAttribute)));
            methodToInject.CustomAttributes.Remove(propertyInjectorAttribute);
            var point = (MethodPoint)propertyInjectorAttribute.ConstructorArguments[1].Value;

            return point == MethodPoint.Beginning ? new[] { injectionTarget.Body.Instructions.First() } : injectionTarget.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret);
        }

        protected override MethodDefinition GetInjectionTarget(MethodDefinition methodToInject, IMemberDefinition member)
        {
            var property = (PropertyDefinition)member;

            var propertyInjectorAttribute = methodToInject.CustomAttributes.First(ca => ca.IsAttributeOfType(typeof(PropertyInjectionAttribute)));
            var method = (PropertyMethod)propertyInjectorAttribute.ConstructorArguments[0].Value;

            return method == PropertyMethod.Get ? property.GetMethod : property.SetMethod;
        }
    }
}