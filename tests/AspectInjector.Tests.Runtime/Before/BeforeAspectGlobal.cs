using AspectInjector.Broker;
using AspectInjector.Tests.Runtime.Before;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Tests.Runtime
{
    internal static partial class TestClassWrapper<T1>
    {
        [Inject(typeof(BeforeAspectGlobal))]
        public partial class TestClass<T2> { }
    }
}

namespace AspectInjector.Tests.Runtime.Before
{
    [Aspect(Aspect.Scope.Global)]
    internal class BeforeAspectGlobal
    {
        public static readonly string BeforeAspectGlobalExecuted = "BeforeAspectGlobalExecuted";

        public static string Executed { get; internal set; }

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
            TestLog.Write(BeforeAspectGlobalExecuted);
        }
    }
}