using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace AspectInjector.BuildTask.Extensions
{
    internal static class CustomAttributeExtensions
    {
        public static bool IsAttributeOfType<T>(this CustomAttribute attribute)
        {
            return attribute.AttributeType.Resolve().FullName == typeof(T).FullName;
        }

        public static bool HasAttributeOfType<T>(this IEnumerable<CustomAttribute> attributes)
        {
            return attributes.Any(a => a.IsAttributeOfType<T>());
        }

        public static IEnumerable<CustomAttribute> GetAttributesOfType<T>(this IEnumerable<CustomAttribute> attributes)
        {
            return attributes.Where(a => a.IsAttributeOfType<T>());
        }

        public static CustomAttribute GetAttributeOfType<T>(this IEnumerable<CustomAttribute> attributes)
        {
            return attributes.GetAttributesOfType<T>().FirstOrDefault();
        }

        public static object GetPropertyValue(this CustomAttribute attribute, string propertyName)
        {
            return attribute.Properties.Where(p => p.Name == propertyName).Select(p => p.Argument.Value).FirstOrDefault();
        }
    }
}
