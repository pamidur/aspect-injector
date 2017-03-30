using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AspectInjector.Tests.Assets
{
    public class TestAspectBase
    {
        internal void LogArguments(object[] args, string prefix)
        {
            if (args == null)
                return;
            else
                foreach (var arg in args)
                {
                    if (arg == null)
                        TestLog.Write($"{prefix}:Arguments:null");
                    else
                        TestLog.Write($"{prefix}:Arguments:{arg.GetType().Name}:{arg.ToString()}");
                }
        }

        internal void LogAttributes(object[] args, string prefix)
        {
            return;
        }

        internal void LogMethod(MethodBase method, string prefix)
        {
            if (method == null)
                TestLog.Write($"{prefix}:Method:null");
            else
                TestLog.Write($"{prefix}:Method:{method.DeclaringType.FullName}+{method.Name}");
        }

        internal void LogInstance(object _this, string prefix)
        {
            if (_this == null)
                TestLog.Write($"{prefix}:Instance:null");
            else
                TestLog.Write($"{prefix}:Instance:{_this.GetType().FullName}:{_this.ToString()}");
        }

        internal void LogName(string name, string prefix)
        {
            if (name == null)
                TestLog.Write($"{prefix}:Name:null");
            else
                TestLog.Write($"{prefix}:Name:{name}");
        }

        internal void LogReturnType(Type retType, string prefix)
        {
            if (retType == null)
                TestLog.Write($"{prefix}:ReturnType:null");
            else
                TestLog.Write($"{prefix}:ReturnType:{retType.FullName}");
        }

        internal void LogReturnValue(object retVal, string prefix)
        {
            if (retVal == null)
                TestLog.Write($"{prefix}:ReturnValue:null");
            else
                TestLog.Write($"{prefix}:ReturnValue:{retVal.GetType().FullName}:{retVal.ToString()}");
        }

        internal void LogTarget(Func<object[], object> target, string prefix)
        {
            //if (target == null)
            //    TestLog.Write($"{prefix}:Target:null");
            //else
            //    TestLog.Write($"{prefix}:Target:{target.Target.GetType().FullName}:{target.ToString()}");
        }

        internal void LogType(Type hostType, string prefix)
        {
            if (hostType == null)
                TestLog.Write($"{prefix}:Type:null");
            else
                TestLog.Write($"{prefix}:Type:{hostType.GetType().FullName}:{hostType.ToString()}");
        }
    }
}