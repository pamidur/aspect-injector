using AspectInjector.Broker;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class AspectContext
    {
        public AspectContext()
        {
        }        

        public CustomAttributeArgument? AspectCustomData { get; set; }

        public MethodDefinition AspectFactory { get; set; }

        public List<AdviceArgumentSource> AspectFactoryArgumentsSources { get; set; }

        public TypeDefinition AspectType { get; set; }

        public TargetMethodContext TargetMethodContext { get; set; }

        public string TargetName { get; set; }

        public TypeDefinition TargetType { get; set; }
    }
}