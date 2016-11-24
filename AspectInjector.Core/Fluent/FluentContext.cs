using AspectInjector.Core.Fluent.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Fluent
{
    internal class FluentContext
    {
        public FluentContext(ModuleDefinition md)
        {
            TypeSystem = new ExtendedTypeSystem(md);
        }

        public ExtendedTypeSystem TypeSystem { get; }

        public FluentMethod GetFluentMember(MethodDefinition md)
        {
        }

        public FluentField GetFluentMember(FieldDefinition md)
        {
        }

        public FluentProperty GetFluentMember(PropertyDefinition md)
        {
        }

        public FluentEvent GetFluentMember(EventDefinition md)
        {
        }

        public FluentType GetFluentMember(TypeDefinition md)
        {
        }
    }
}