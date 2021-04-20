using System;
using System.Linq;
using AspectInjector.Broker;

namespace Aspects.StringLengthCheck
{
    [Aspect(Scope.Global)]
    public class PropertyCheckAspect
    {
        [Advice(Kind.Before, Targets = Target.Instance | Target.Public | Target.Setter)]
        public void OnSet([Argument(Source.Triggers)] Attribute[] attributes,
            [Argument(Source.Arguments)] object[] arguments, [Argument(Source.Name)] string name)
        {
            var checkLength = attributes.Reverse().OfType<StringLengthCheckAttribute>().FirstOrDefault();

            if (checkLength != null)
            {
                if (((string)arguments[0]).Length > checkLength.MaximumLength ||
                    ((string)arguments[0]).Length < checkLength.MinimumLength)
                {
                    throw new ArgumentException($"{name} length must >= {checkLength.MinimumLength} and <= {checkLength.MaximumLength}");
                }
            }
        }
    }

    [Injection(typeof(PropertyCheckAspect), Propagation = PropagateTo.Properties)]
    public class StringLengthCheckAttribute : Attribute
    {
        public uint MaximumLength { get; set; }

        public uint MinimumLength { get; set; }

        public StringLengthCheckAttribute(uint maximumLength = 0, uint minimumLength = 0)
        {
            MaximumLength = maximumLength;
            MinimumLength = minimumLength;
        }
    }
}
