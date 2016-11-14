using AspectInjector.Contracts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Contexts
{
    public class ProcessingContext
    {
        public AssemblyDefinition Assembly { get; internal set; }
        public ILogger Log { get; set; }
    }
}