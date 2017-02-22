using AspectInjector.Broker;
using System;
using System.Reflection;

namespace AspectInjector.Tests.Runtime.Before.Global
{
    [Aspect(Aspect.Scope.Global)]
    internal class BeforeAspectGlobal : TestAspectBase
    {
        public static readonly string Executed = "BeforeAspectGlobal";

        [Advice(Advice.Type.Before, Advice.Target.Constructor | Advice.Target.EventAdd | Advice.Target.EventRemove | Advice.Target.Getter | Advice.Target.Method | Advice.Target.Setter)]
        public void Before(
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
            [Advice.Argument(Advice.Argument.Source.Attributes)] Attribute[] attrs,
            [Advice.Argument(Advice.Argument.Source.Instance)] object _this,
            [Advice.Argument(Advice.Argument.Source.Method)] MethodBase method,
            [Advice.Argument(Advice.Argument.Source.Name)] string name,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type retType,
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] object retVal,
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Type)] Type hostType
            )
        {
            base.LogArguments(args, Executed);
            base.LogAttributes(args, Executed);
            base.LogInstance(_this, Executed);
            base.LogMethod(method, Executed);
            base.LogName(name, Executed);
            base.LogReturnType(retType, Executed);
            base.LogReturnValue(retVal, Executed);
            base.LogTarget(target, Executed);
            base.LogType(hostType, Executed);

            TestLog.Write(Executed);
        }
    }
}