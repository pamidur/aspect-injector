using AspectInjector.Broker;
using System;
using System.Collections.Generic;

namespace Aspects.Lazy
{
    [Aspect(Scope.PerInstance)]
    public sealed class LazyAspect
    {
        private readonly Dictionary<string, object> _backFields = new Dictionary<string, object>();

        [Advice(Kind.Around, Targets = Target.Public | Target.Getter)]
        public object OnGet([Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Type)] Type type, [Argument(Source.Name)] string name)
        {
            var key = GetKey(type.FullName, name);
            if (!_backFields.TryGetValue(key, out object value))
            {
                lock (_backFields)
                {
                    if (!_backFields.TryGetValue(key, out value))
                    {
                        value = method(Array.Empty<object>());
                        _backFields.Add(key, value);
                    }
                }
            }

            return value;

            string GetKey(string typeName, string targetName)
            {
                return $"{typeName}.{targetName}";
            }
        }
    }
}