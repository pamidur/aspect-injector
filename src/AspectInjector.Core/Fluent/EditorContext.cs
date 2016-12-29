using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Fluent
{
    public class EditorContext
    {
        public ExtendedTypeSystem TypeSystem { get; private set; }
        public EditorFactory Factory { get; private set; }

        internal AspectFactory Aspects { get; private set; }
        internal ModuleDefinition Module { get; private set; }

        public EditorContext(ModuleDefinition md, EditorFactory factory)
        {
            Module = md;
            TypeSystem = new ExtendedTypeSystem(md);
            Factory = factory;
            Aspects = new AspectFactory(this);
        }
    }
}