using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime
{
    public class TestAspectBase
    {
        internal void LogArgs(object[] args)
        {
            if (args == null)
                return;
            else
                foreach (var arg in args)
                {
                    if (arg == null)
                        TestLog.Write($"Arguments:null");
                    else
                        TestLog.Write($"Arguments:{arg.GetType().Name}:{arg.ToString()}");
                }
        }
    }
}