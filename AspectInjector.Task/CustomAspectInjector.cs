using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace AspectInjector.BuildTask
{
    internal class CustomAspectInjector : InjectorBase, IAspectInjector
    {
        public void ProcessModule(ModuleDefinition module)
        {
            foreach (var @class in module.Types.Where(t => t.IsClass))
            {
                var classAttribute = @class.CustomAttributes.SingleOrDefault(a => a.IsAttributeOfType(typeof(AspectAttribute)));
                //todo::  think about configurable filtering of method types
                foreach (var method in @class.Methods.Where(m => !m.IsGetter && !m.IsSetter && !m.IsConstructor && !m.IsAbstract))
                {
                    var methodAttribute = method.CustomAttributes.SingleOrDefault(a => a.IsAttributeOfType(typeof(AspectAttribute)));
                    ProcessMethod(module, method, methodAttribute ?? classAttribute);
                }
            }
        }

        private void ProcessMethod(ModuleDefinition module, MethodDefinition method, CustomAttribute attribute)
        {
            if (attribute != null)
            {
                var aspectType = (TypeDefinition)attribute.ConstructorArguments[0].Value;

                if (aspectType.Interfaces.HasType(typeof(ICustomAspect)))
                {
                    var aspectInstanseReference = GetOrCreateAspectReference(method.DeclaringType, aspectType);

                    MethodReference beforeMethod = module.ImportMethod<ICustomAspect>(aspectType, a => a.Before());
                    MethodReference afterMethod = module.ImportMethod<ICustomAspect>(aspectType, a => a.After());

                    ILProcessor processor = method.Body.GetILProcessor();
                    Instruction firstInstruction = method.Body.Instructions.First();
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Nop));
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Ldfld, aspectInstanseReference));
                    processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Callvirt, beforeMethod));

                    //todo:: process all return statements (all code execution pathes)
                    Instruction lastInstruction = method.Body.Instructions.Last();
                    processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Ldarg_0));
                    processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Ldfld, aspectInstanseReference));
                    processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Callvirt, afterMethod));
                    processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Nop));
                }
            }
        }
    }
}