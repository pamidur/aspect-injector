using Mono.Cecil;
using Mono.Collections.Generic;

namespace FluentIL.Extensions
{
    public static class GenericsExtensions
    {
        public static void CloneTo(this Collection<GenericParameter> from, IGenericParameterProvider to)
        {
            foreach (var from_param in from)
            {
                to.GenericParameters.Add(new GenericParameter(from_param.Name, to)
                {
                    Attributes = from_param.Attributes,
                    IsValueType = from_param.IsValueType
                });
            }

            foreach (var from_param in from)
            {
                var to_param = to.GenericParameters[from_param.Position];
                foreach (var from_constraint in from_param.Constraints)
                {
                    to_param.Constraints.Add(new GenericParameterConstraint(to.Module.ImportReference(from_constraint.ConstraintType, to)));
                }
            }
        }
    }
}
