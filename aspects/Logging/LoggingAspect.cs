using AspectInjector.Broker;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;

namespace Aspects.Logging
{
    [Aspect(Scope.Global)]
    public class LoggingAspect
    {
        static LoggingAspect()
        {
            LoggerFactory =
            Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                builder.AddConsole());
        }

        public static ILoggerFactory LoggerFactory { get; set; }

        [Advice(Kind.Around)]
        public object Around(
            [Argument(Source.Name)] string name,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Type)] Type hostType,
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Triggers)] Attribute[] triggers)
        {
            var trigger = triggers.OfType<LogAttribute>().First();

            var logger = LoggerFactory.CreateLogger(hostType);
            logger.LogInformation("Executing method {method}", name);


            if (trigger.MeasureTime)
            {
                var sw = Stopwatch.StartNew();

                var result = target(args);

                sw.Stop();
                logger.LogInformation("Executed method {method} in {time} ms", name, sw.ElapsedMilliseconds);                
                return result;
            }
            else
            {
                var result = target(args);

                logger.LogInformation("Executed method {method}", name);               
                return result;
            }
        }
    }
}
