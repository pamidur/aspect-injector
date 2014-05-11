using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    public class AsyncAspect
    {
        [Advice(InjectionPoints.Before,InjectionTargets.Method)]
        public void Before()
        {
            Console.WriteLine("Before");
        }

        [Advice(InjectionPoints.After, InjectionTargets.Method)]
        public void After()
        {
            Console.WriteLine("After");
        }
    }

    class AsyncTests
    {
        [Aspect(typeof(AsyncAspect))]
        public async Task Do()
        {
            var a = 1;
            await Task.Delay(1);
            a = 2;
            await Task.Delay(1);
            a = 3;
        }        
    }

    class AsyncTests2
    {
        private readonly AsyncAspect aa = new AsyncAspect();

        public async Task<string> Do()
        {
            aa.Before();
            await Task.Delay(1);
            aa.After();

            return "fff";
        }
    }
}
