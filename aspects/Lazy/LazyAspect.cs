using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aspects.Lazy
{
    [Aspect(Scope.PerInstance)]
    public sealed class LazyAspect
    {
        private readonly Dictionary<string, object> _backFields = new Dictionary<string, object>();

        [Advice(Kind.Around, Targets = Target.Public | Target.Getter)]
        public object OnGet([Argument(Source.Target)] Func<object[], object> method, [Argument(Source.Type)] Type type, [Argument(Source.Name)] string name)
        {
            var key = GetKey(type.IsGenericType ? method.Method.DeclaringType : type, name);

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

            string GetKey(Type declareType, string targetName)
            {
                return $"{declareType.FullName}.{targetName}";
            }
        }
    }
}