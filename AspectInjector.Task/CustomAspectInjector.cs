using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask
{
    internal class CustomAspectInjector : InjectorBase, IAspectInjector
    {
        public void ProcessModule(ModuleDefinition module)
        {
            var allMethods = module.Types
                .Where(t => t.IsClass)
                .SelectMany(t => t.Methods);

            foreach (var method in allMethods)
            {
                var attribute = method.CustomAttributes.SingleOrDefault(a => a.IsAttributeOfType(typeof(AspectAttribute)));
                if (attribute != null)
                {
                    var aspectType = (TypeDefinition)attribute.Properties.First(p => p.Name == "Type").Argument.Value;

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
}
