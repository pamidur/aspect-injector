using System;
using System.Linq;
using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Contexts
{
    public class AspectContext
    {
        public CustomAttribute[] AspectRoutableData { get; }

        public Lazy<MethodDefinition> AdviceClassFactory { get; }

        public TypeDefinition AdviceClassType { get; }

        public AspectScope AdviceClassScope { get; }

        public Lazy<TargetMethodContext> TargetMethodContext { get; }

        public string TargetName { get; }

        public TargetTypeContext TargetTypeContext { get; }

        public AspectContext(MethodDefinition targetMethod, string targetName, TypeDefinition adviceClassType, CustomAttribute[] routableData)
        {
            TargetName = targetName;
            TargetTypeContext = TypeContextFactory.GetOrCreateContext(targetMethod.DeclaringType);
            AdviceClassType = adviceClassType;
            AdviceClassScope = GetAspectScope(targetMethod, adviceClassType);
            AspectRoutableData = routableData;
            TargetMethodContext = new Lazy<TargetMethodContext>(() => MethodContextFactory.GetOrCreateContext(targetMethod));
            AdviceClassFactory = new Lazy<MethodDefinition>(GetAdviceClassFactory);
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