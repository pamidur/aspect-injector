using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Fluent
{
    public class FluentContext
    {
        public FluentContext(ModuleDefinition md)
        {
            TypeSystem = new ExtendedTypeSystem(md);
        }

        public ExtendedTypeSystem TypeSystem { get; }
    }
}