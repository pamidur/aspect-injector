using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask
{
    public class MethodAspectInjector : InjectorBase, IAspectInjector
    {
        protected virtual MethodDefinition GetInjectionTarget(MethodDefinition methodToInject, IMemberDefinition member)
        {
            return (MethodDefinition)member;
        }

        protected virtual IEnumerable<Instruction> GetInjectionPoints(MethodDefinition methodToInject, MethodDefinition injectionTarget)
        {
            var propertyInjectorAttribute = methodToInject.CustomAttributes.First(ca => ca.IsAttributeOfType(typeof(MethodInjectionAttribute)));
            var point = (MethodPoint)propertyInjectorAttribute.Properties.First(p => p.Name == "Point").Argument.Value;

            return point == MethodPoint.Begining ? new[] { injectionTarget.Body.Instructions.First() } : injectionTarget.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret);
        }

        public void ProcessModule(Mono.Cecil.ModuleDefinition module)
        {
            //todo:: make it be able to get all properties of marked class

            //Get properties marked for aspect
            var allMethods = module.Types
                .Where(t => t.IsClass)
                .SelectMany(t => t.Methods).Where(p => p.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(AspectAttribute))));

            ProcessDefinitions(allMethods);
        }

        protected virtual IEnumerable<MethodDefinition> GetInjectableAspectMethods(TypeDefinition aspectType)
        {
            return aspectType.Methods.Where(m => m.CustomAttributes.Any(ca => ca.IsAttributeOfType(typeof(MethodInjectionAttribute))));
        }

        protected void ProcessDefinitions<T>(IEnumerable<T> allDefinitions)
            where T : IMemberDefinition
        {
            foreach (var member in allDefinitions)
            {
                //there could be several aspects per one member
                var aspectAttributes = member.CustomAttributes.Where(a => a.IsAttributeOfType(typeof(AspectAttribute)));

                foreach (var aspectAttribute in aspectAttributes)
                {
                    //get the actual aspect
                    var aspectType = (TypeDefinition)aspectAttribute.Properties.First(p => p.Name == "Type").Argument.Value;

                    //create a reference to aspect as private field
                    var aspectInstanseReference = GetOrCreateAspectReference(member.DeclaringType, aspectType);

                    //System.Diagnostics.Debugger.Launch();

                    //looking for methods in aspect which should be injected
                    var methodsToInject = GetInjectableAspectMethods(aspectType);
                    foreach (var methodToInject in methodsToInject)
                    {
                        //figuring out get or set method
                        var injectionTaregt = GetInjectionTarget(methodToInject, member);

                        //there could be several return statemets
                        var injectionPoints = GetInjectionPoints(methodToInject, injectionTaregt).ToList();

                        foreach (var injectionPoint in injectionPoints)
                        {
                            ILProcessor processor = injectionTaregt.Body.GetILProcessor();
                            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Nop));
                            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldarg_0));
                            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldfld, aspectInstanseReference));

                            foreach (var argument in methodToInject.Parameters)
                            {
                                var injectArgumetAttribute = argument.CustomAttributes.FirstOrDefault(ca => ca.IsAttributeOfType(typeof(InjectArgumentAttribute)));

                                if (injectArgumetAttribute == null)
                                    throw new NotSupportedException("Cannot inject unknown argument");

                                var injectionArgument = (InjectArgument)injectArgumetAttribute.Properties.First(p => p.Name == "Argument").Argument.Value;

                                if (injectionArgument == InjectArgument.Instanse && argument.ParameterType.IsType(typeof(object)))
                                {
                                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldarg_0));
                                    continue;
                                }

                                if (injectionArgument == InjectArgument.MemberName && argument.ParameterType.IsType(typeof(string)))
                                {
                                    processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Ldstr, member.Name));
                                    continue;
                                }

                                throw new NotSupportedException("not supported yet");
                            }

                            processor.InsertBefore(injectionPoint, processor.Create(OpCodes.Callvirt, methodToInject));
                        }
                    }
                }
            }
        }
    }
}