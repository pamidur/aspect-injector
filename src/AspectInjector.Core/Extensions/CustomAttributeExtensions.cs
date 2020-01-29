using FluentIL.Extensions;
using Mono.Cecil;
using System.Linq;

namespace AspectInjector.Core.Extensions
{
    public static class CustomAttributeExtensions
    {
        public static bool IsCompilerGenerated(this ICustomAttributeProvider t)
        {
            return t.CustomAttributes.Any(ca => ca.AttributeType.Match(WellKnownTypes.CompilerGeneratedAttribute));
        }

        public static TR GetPropertyValue<TR>(this CustomAttribute ca, string name)
        {
            var prop = ca.Properties.Where(p => p.Name == name).ToList();

            if (prop.Count != 1)
                return default;

            return (TR) prop[0].Argument.Value;
        }

        public static TR GetConstructorValue<TR>(this CustomAttribute ca, int argPosition)
        {
            if (argPosition + 1 > ca.ConstructorArguments.Count)
                return default;

            var prop = ca.ConstructorArguments[argPosition];
            return (TR)prop.Value;
        }
    }
}