using Mono.Cecil;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspectInjector.Core.Extensions
{
    public static class CustomAttributeExtensions
    {
        public static TR GetPropertyValue<TA, TR>(this CustomAttribute ca, Expression<Func<TA, TR>> getter)
        {
            var body = getter.Body as MemberExpression;

            var prop = ca.Properties.Where(p => p.Name == body.Member.Name).ToList();

            if (prop.Count != 1)
                return default(TR);

            return (TR)prop[0].Argument.Value;
        }

        public static TR GetFieldValue<TA, TR>(this CustomAttribute ca, Expression<Func<TA, TR>> getter)
        {
            var body = getter.Body as MemberExpression;

            var prop = ca.Fields.Where(f => f.Name == body.Member.Name).ToList();

            if (prop.Count != 1)
                return default(TR);

            return (TR)prop[0].Argument.Value;
        }

        public static TR GetConstructorValue<TR>(this CustomAttribute ca, int argPosition)
        {
            if (argPosition + 1 > ca.ConstructorArguments.Count)
                return default(TR);

            var prop = ca.ConstructorArguments[argPosition];
            return (TR)prop.Value;
        }
    }
}