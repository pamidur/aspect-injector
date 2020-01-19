using AspectInjector.Broker;
using System;
using System.Diagnostics;

namespace AspectInjector.Samples.Logging.Aspects
{
    [Aspect(Scope.Global)]
    [Injection(typeof(Log))]
    public class Log : Attribute
    {
        [Advice(Kind.Around, Targets = Target.Method)]
        public object HandleMethod(
            [Argument(Source.Name)] string name,
            [Argument(Source.Arguments)] object[] arguments,
            [Argument(Source.Target)] Func<object[], object> method)
        {
            Console.WriteLine($"Executing method {name}");

            var sw = Stopwatch.StartNew();

            var result = method(arguments);

            sw.Stop();

            Console.WriteLine($"Executed method {name} in {sw.ElapsedMilliseconds} ms");

            return result;
        }
    }
}
