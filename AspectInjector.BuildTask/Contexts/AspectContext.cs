using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
    public class AspectContext
    {
        private MethodDefinition _adviceClassFactory = null;

        public AspectContext()
        {
        }

        public object AspectRoutableData { get; set; }

        public MethodDefinition AdviceClassFactory
        {
            get
            {
                if (_adviceClassFactory == null)
                {
                    var aspectFactories = AdviceClassType.Methods.Where(m => m.IsStatic && !m.IsConstructor && m.CustomAttributes.HasAttributeOfType<AspectFactoryAttribute>()).ToList();

                    if (aspectFactories.Any())
                        _adviceClassFactory = aspectFactories[0];
                    else
                    {
                        _adviceClassFactory = AdviceClassType.Methods
                        .Where(c => c.IsConstructor && !c.IsStatic && !c.Parameters.Any())
                        .FirstOrDefault();
                    }
                }

                return _adviceClassFactory;
            }
        }

        public TypeDefinition AdviceClassType { get; set; }

        public AspectScope AdviceClassScope { get; set; }

        public TargetMethodContext TargetMethodContext { get; set; }

        public string TargetName { get; set; }

        public TargetTypeContext TargetTypeContext { get; set; }
    }
}