using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test.CodeContracts
{

    class Test
    {
        private StringBuilder dfdf = new StringBuilder();

        [Aspect(typeof(AspectClass))]
        public Test(int a)
        {
            //Contract.Requires<ArgumentException>(a > 0);
        }
    }

    class AspectClass
    {
        [Advice(InjectionPoints.Before, InjectionTargets.Method | InjectionTargets.Constructor)]
        public void Before()
        {

        }
    }
}
