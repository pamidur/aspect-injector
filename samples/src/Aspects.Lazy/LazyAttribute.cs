using System;
using System.Collections.Generic;
using AspectInjector.Broker;

namespace Aspects.Lazy
{
    [Aspect(Scope.PerInstance)]
    public class LazyAspect
    {
        private readonly Dictionary<string, object> _backFields = new Dictionary<string, object>();

        [Advice(Kind.Around, Targets = Target.Instance | Target.Public | Target.Getter)]
        public object OnGet([Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Name)] string name,
            [Argument(Source.Arguments)] object[] arguments)
        {
            if (!_backFields.TryGetValue(name, out object value))
            {
                var newValue = method(arguments);
                if (_backFields.TryGetValue(name, out object created))
                {
                    value = created;
                }
                else
                {
                    _backFields.Add(name, newValue);
                    value = newValue;
                }
            }

            return value;
        }
    }

    [Injection(typeof(LazyAspect), Propagation = PropagateTo.Properties)]
    public class LazyAttribute : Attribute
    {

    }
}
