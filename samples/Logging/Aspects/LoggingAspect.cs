using AspectInjector.Broker;
using System;
using System.Diagnostics;

namespace AspectInjector.Samples.Logging.Aspects
{
    [Aspect(Aspect.Scope.Global)]
    class LoggingAspect
    {
        [Advice(Advice.Type.Around, Advice.Target.Method)]
        public object HandleMethod([Advice.Argument(Advice.Argument.Source.Name)] string name,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments,
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> method)
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
