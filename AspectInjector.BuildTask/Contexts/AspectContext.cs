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

        public AspectContext(AspectContext other)
        {
            AspectType = other.AspectType;
            AspectFactory = other.AspectFactory;
            AspectFactoryArgumentsSources = other.AspectFactoryArgumentsSources;
            AspectCustomData = other.AspectCustomData;

            TargetType = other.TargetType;
            TargetMethodContext = other.TargetMethodContext;
            TargetName = other.TargetName;
        }


        public object[] AspectCustomData { get; set; }
        public MethodDefinition AspectFactory { get; set; }
        public List<AdviceArgumentSource> AspectFactoryArgumentsSources { get; set; }
        public TypeDefinition AspectType { get; set; }
        
        public TargetMethodContext TargetMethodContext { get; set; }
        public string TargetName { get; set; }
        public TypeDefinition TargetType { get; set; }        
    }
}