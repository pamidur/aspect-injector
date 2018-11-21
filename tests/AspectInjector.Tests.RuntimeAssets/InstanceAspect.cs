using AspectInjector.Broker;
using System;
using System.Reflection;

namespace AspectInjector.Tests.Assets
{
    [Aspect(Aspect.Scope.PerInstance)]
    [Mixin(typeof(IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>))]
    public class InstanceAspect : TestAspectBase, IAssetIface1Wrapper<Asset1>.IAssetIface1<Asset2>
    {
        public static readonly string AfterExecuted = "AfterAspectInstance";
        public static readonly string BeforeExecuted = "BeforeAspectInstance";
        public static readonly string AroundEnter = "AroundAspectInstanceEnter";
        public static readonly string AroundExit = "AroundAspectInstanceExit";

        [Advice(Advice.Kind.After)]
        public void After(
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
            //[Advice.Argument(Advice.Argument.Source.Attributes)] Attribute[] attrs,
            [Advice.Argument(Advice.Argument.Source.Instance)] object _this,
            [Advice.Argument(Advice.Argument.Source.Method)] MethodBase method,
            [Advice.Argument(Advice.Argument.Source.Name)] string name,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type retType,
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] object retVal,
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Type)] Type hostType
            )
        {
            base.LogArguments(args, AfterExecuted);
            base.LogAttributes(args, AfterExecuted);
            base.LogInstance(_this, AfterExecuted);
            base.LogMethod(method, AfterExecuted);
            base.LogName(name, AfterExecuted);
            base.LogReturnType(retType, AfterExecuted);
            base.LogReturnValue(retVal, AfterExecuted);
            base.LogTarget(target, AfterExecuted);
            base.LogType(hostType, AfterExecuted);

            TestLog.Write(AfterExecuted);
        }

        [Advice(Advice.Kind.Before)]
        public void Before(
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
            //[Advice.Argument(Advice.Argument.Source.Attributes)] Attribute[] attrs,
            [Advice.Argument(Advice.Argument.Source.Instance)] object _this,
            [Advice.Argument(Advice.Argument.Source.Method)] MethodBase method,
            [Advice.Argument(Advice.Argument.Source.Name)] string name,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type retType,
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] object retVal,
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Type)] Type hostType
            )
        {
            base.LogArguments(args, BeforeExecuted);
            ///base.LogAttributes(attrs, BeforeExecuted);
            base.LogInstance(_this, BeforeExecuted);
            base.LogMethod(method, BeforeExecuted);
            base.LogName(name, BeforeExecuted);
            base.LogReturnType(retType, BeforeExecuted);
            base.LogReturnValue(retVal, BeforeExecuted);
            base.LogTarget(target, BeforeExecuted);
            base.LogType(hostType, BeforeExecuted);

            TestLog.Write(BeforeExecuted);
        }

        [Advice(Advice.Kind.Around)]
        public object Around(
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] args,
            //[Advice.Argument(Advice.Argument.Source.Attributes)] Attribute[] attrs,
            [Advice.Argument(Advice.Argument.Source.Instance)] object _this,
            [Advice.Argument(Advice.Argument.Source.Method)] MethodBase method,
            [Advice.Argument(Advice.Argument.Source.Name)] string name,
            [Advice.Argument(Advice.Argument.Source.ReturnType)] Type retType,
            [Advice.Argument(Advice.Argument.Source.ReturnValue)] object retVal,
            [Advice.Argument(Advice.Argument.Source.Target)] Func<object[], object> target,
            [Advice.Argument(Advice.Argument.Source.Type)] Type hostType
            )
        {
            base.LogArguments(args, AroundEnter);
            //base.LogAttributes(attrs, AroundEnter);
            base.LogInstance(_this, AroundEnter);
            base.LogMethod(method, AroundEnter);
            base.LogName(name, AroundEnter);
            base.LogReturnType(retType, AroundEnter);
            base.LogReturnValue(retVal, AroundEnter);
            base.LogTarget(target, AroundEnter);
            base.LogType(hostType, AroundEnter);

            TestLog.Write(AroundEnter);
            var result = target(args);
            TestLog.Write(AroundExit);
            return result;
        }

        public Tuple<int, Asset1, Asset1, Asset2, T3> TestMethod<T3>(int a1, Asset1 a2, Asset1 a3, Asset2 a4, T3 a5, ref int ar1, ref Asset1 ar2, ref Asset1 ar3, ref Asset2 ar4, ref T3 ar5, out int ao1, out Asset1 ao2, out Asset1 ao3, out Asset2 ao4, out T3 ao5)
        {
            throw new NotImplementedException();
        }

        public void EmptyMethod()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<Tuple<Asset1, Asset2>> TestEvent;

        public Tuple<Asset1, Asset2> TestProperty
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}