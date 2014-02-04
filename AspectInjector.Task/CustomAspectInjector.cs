using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask
{
    internal class CustomAspectInjector : IAspectInjector
    {
        public void ProcessModule(ModuleDefinition module)
        {
            var allMethods = module.Types
                .Where(t => t.IsClass)
                .SelectMany(t => t.Methods);

            foreach (var method in allMethods)
            {
                var attribute = method.CustomAttributes.SingleOrDefault(a => a.IsAttributeOfType(typeof(CustomAspectAttribute)));
                if (attribute != null)
                {
                    //TODO: Get property by name
                    var aspectType = (TypeDefinition)attribute.Properties.First().Argument.Value;
                    if (aspectType.Interfaces.HasType(typeof(ICustomAspect)))
                    {
                        MethodReference constructor = module.ImportConstructor(aspectType);
                        MethodReference beforeMethod = module.ImportMethod<ICustomAspect>(aspectType, a => a.Before());
                        MethodReference afterMethod = module.ImportMethod<ICustomAspect>(aspectType, a => a.After());

                        ILProcessor processor = method.Body.GetILProcessor();
                        Instruction firstInstruction = method.Body.Instructions.First();
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Nop));
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Newobj, constructor));
                        processor.InsertBefore(firstInstruction, processor.Create(OpCodes.Callvirt, beforeMethod));

                        //TODO: Optimize injecting by creating aspect instance only once
                        Instruction lastInstruction = method.Body.Instructions.Last();
                        processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Newobj, constructor));
                        processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Callvirt, afterMethod));
                        processor.InsertBefore(lastInstruction, processor.Create(OpCodes.Nop));
                    }
                }
            }
        }
    }
}
