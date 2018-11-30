using AspectInjector.Broker;
using System;
using System.Reflection;

namespace AspectInjector.Tests.Assets
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [Injection(typeof(GlobalAspect))]
    public class InjectGlobalAspect : Attribute
    {

    }

    [Aspect(Aspect.Scope.Global)]
    [Mixin(typeof(IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>))]
    public class GlobalAspect : TestAspectBase, IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>
    {
        public static readonly string AfterExecuted = "AfterAspectGlobal";
        public static readonly string BeforeExecuted = "BeforeAspectGlobal";
        public static readonly string AroundEnter = "AroundAspectGlobalEnter";
        public static readonly string AroundExit = "AroundAspectGlobalExit";

        

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
            //base.LogAttributes(args, AfterExecuted);
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
            //base.LogAttributes(args, BeforeExecuted);
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
            base.LogAttributes(args, AroundEnter);
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

        Tuple<int, Asset1, Asset2, Asset1, T3> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestMethod<T3>(int a1, Asset1 a2, Asset2 a3, Asset1 a4, T3 a5, ref int ar1, ref Asset1 ar2, ref Asset2 ar3, ref Asset1 ar4, ref T3 ar5, out int ao1, out Asset1 ao2, out Asset2 ao3, out Asset1 ao4, out T3 ao5)
        {
            throw new NotImplementedException();
        }

        void IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.EmptyMethod()
        {
            throw new NotImplementedException();
        }

        Tuple<Asset2, Asset1> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestProperty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        event EventHandler<Tuple<Asset2, Asset1>> IAssetIface1Wrapper<Asset2>.IAssetIface1<Asset1>.TestEvent
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }
    }
}