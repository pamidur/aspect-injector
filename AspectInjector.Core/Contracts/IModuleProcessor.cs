using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Contracts
{
    public interface IModuleProcessor : IInitializable
    {
        void ProcessModule(ModuleDefinition module);
    }
}