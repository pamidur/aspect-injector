using AspectInjector.Broker;
using System;
using System.Collections.Generic;

namespace Aspects.Lazy
{
    [Aspect(Scope.PerInstance)]
    public class LazyAspect
    {
        private readonly Dictionary<string, object> _backFields = new Dictionary<string, object>();

        [Advice(Kind.Around, Targets = Target.Instance | Target.Public | Target.Getter)]
        public object OnGet([Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Name)] string name)
        {
            if (!_backFields.TryGetValue(name, out object value))
            {
                lock (_backFields)
                {
                    if (!_backFields.TryGetValue(name, out value))
                    {
                        value = method(Array.Empty<object>());
                        _backFields.Add(name, value);
                    }
                }
            }

            return value;
        }
    }
}