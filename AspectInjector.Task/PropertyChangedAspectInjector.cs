using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.BuildTask
{
    internal class PropertyChangedAspectInjector : IAspectInjector
    {
        public void ProcessModule(ModuleDefinition module)
        {
            var allProperties = module.Types
                .Where(t => t.IsClass)
                .SelectMany(t => t.Properties)
                .Where(p => p.CustomAttributes.HasAttributeOfType(typeof(PropertyChangedAspectAttribute)));

            foreach (var property in allProperties)
            {
                var method = property.SetMethod;
                ILProcessor processor = method.Body.GetILProcessor();
                Instruction lastInstruction = method.Body.Instructions.Last();
            }
        }
    }
}
