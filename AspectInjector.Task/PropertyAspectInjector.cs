using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask
{
    public class PropertyAspectInjector : MethodAspectInjector, IAspectInjector
    {
        protected override MethodDefinition GetInjectionTarget(MethodDefinition methodToInject, IMemberDefinition member)
        {
            var property = (PropertyDefinition)member;

            var propertyInjectorAttribute = methodToInject.CustomAttributes.First(ca => ca.IsAttributeOfType(typeof(PropertyInjectionAttribute)));
            var method = (PropertyMethod)propertyInjectorAttribute.Properties.First(p => p.Name == "Method").Argument.Value;

            return method == PropertyMethod.Get ? property.GetMethod : property.SetMethod;
        }

        protected override IEnumerable<Instruction> GetInjectionPoints(MethodDefinition methodToInject, MethodDefinition injectionTarget)
        {
            var propertyInjectorAttribute = methodToInject.CustomAttributes.First(ca => ca.IsAttributeOfType(typeof(PropertyInjectionAttribute)));
            var point = (MethodPoint)propertyInjectorAttribute.Properties.First(p => p.Name == "Point").Argument.Value;

            return point == MethodPoint.Begining ? new[] { injectionTarget.Body.Instructions.First() } : injectionTarget.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret);
        }

        protected override IEnumerable<MethodDefinition> GetInjectableAspectMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(PropertyInjectionAttribute))));
        }

        public void ProcessModule(Mono.Cecil.ModuleDefinition module)
        {
            //todo:: make it be able to get all properties of marked class

            //Get properties marked for aspect
            var allProperties = module.Types
                .Where(t => t.IsClass)
                .SelectMany(t => t.Properties).Where(p => p.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(AspectAttribute))));

            ProcessDefinitions(allProperties);
        }
    }
}