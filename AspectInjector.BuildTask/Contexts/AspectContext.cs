using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Contexts
{
    public class AspectContext
    {
        private MethodDefinition _adviceClassFactory;

        public CustomAttribute[] AspectRoutableData { get; }

        public MethodDefinition AdviceClassFactory
        {
            get { return _adviceClassFactory ?? (_adviceClassFactory = GetAdviceClassFactory()); }
        }

        public TypeDefinition AdviceClassType { get; }

        public AspectScope AdviceClassScope { get; }

        public TargetMethodContext TargetMethodContext { get; set; }

        public string TargetName { get; }

        public TargetTypeContext TargetTypeContext { get; }

        public AspectContext(MethodDefinition targetMethod, string targetName, TypeDefinition adviceClassType, CustomAttribute[] routableData)
        {
            TargetName = targetName;
            TargetTypeContext = TypeContextFactory.GetOrCreateContext(targetMethod.DeclaringType);
            AdviceClassType = adviceClassType;
            AdviceClassScope = GetAspectScope(targetMethod, adviceClassType);
            AspectRoutableData = routableData;
        }

        internal static bool IsAspectFactory(MethodDefinition method)
        {
            return method.IsStatic && !method.IsConstructor && method.CustomAttributes.HasAttributeOfType<AspectFactoryAttribute>();
        }

        private static bool IsDefaultConstructor(MethodDefinition method)
        {
            return method.IsConstructor && !method.IsStatic && !method.Parameters.Any();
        }

        private static AspectScope GetAspectScope(MethodDefinition targetMethod, TypeDefinition adviceClassType)
        {
            var customAttributes = adviceClassType.CustomAttributes;
            if (customAttributes.HasAttributeOfType<AspectScopeAttribute>())
                return (AspectScope)customAttributes.GetAttributeOfType<AspectScopeAttribute>().ConstructorArguments[0].Value;

            return targetMethod.IsStatic ? AspectScope.Type : AspectScope.Instance;
        }

        private MethodDefinition GetAdviceClassFactory()
        {
            return AdviceClassType.Methods.FirstOrDefault(IsAspectFactory) ?? AdviceClassType.Methods.FirstOrDefault(IsDefaultConstructor);
        }
    }
}