using AspectInjector.Core.Fluent;
using Mono.Cecil;

namespace AspectInjector.Core.Extensions
{
    public static class ClonningExtensions
    {
        public static GenericParameter Clone(this GenericParameter gparam, IGenericParameterProvider target, ExtendedTypeSystem ts)
        {
            var ngp = new GenericParameter(gparam.Name, target)
            {
                HasDefaultConstructorConstraint = gparam.HasDefaultConstructorConstraint,
                HasReferenceTypeConstraint = gparam.HasReferenceTypeConstraint,
                HasNotNullableValueTypeConstraint = gparam.HasNotNullableValueTypeConstraint,
                IsNonVariant = gparam.IsNonVariant,
                IsContravariant = gparam.IsContravariant,
                IsCovariant = gparam.IsCovariant,
                //Namespace = gparam.Namespace,
                IsValueType = gparam.IsValueType
            };

            foreach (var gc in gparam.Constraints)
                ngp.Constraints.Add(ts.Import(gc));

            foreach (var subgp in gparam.GenericParameters)
                ngp.GenericParameters.Add(subgp.Clone(target,ts));

            return ngp;
        }
    }
}
