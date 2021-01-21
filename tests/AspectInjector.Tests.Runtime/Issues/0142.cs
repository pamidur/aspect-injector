using AspectInjector.Broker;
using AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspectInjector.Tests.Runtime.Issues
{
    public class Issue_0142
    {
        [TestInjection]
        public class WierdContraintsClass<T, U> : RuntimeAssets.CrossAssemblyHelpers.BaseGenericClass<T>.NestedGenericClass<U>.NestedGeneric2Class<string>
            where T : AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers.TestBaseClass<U>
            where U : IAsyncResult
        {
            public Tuple<U, G> Get<G>(T data)
                where G : RuntimeAssets.CrossAssemblyHelpers.BaseGenericClass<T>.NestedGenericClass<U>.NestedGeneric2Class<G>
            {
                return default;
            }
        }

        [Aspect(Scope.Global)]
        [Injection(typeof(TestInjection))]
        [Mixin(typeof(BaseGenericClass<int>.NestedGenericClass<string>.NestedGeneric2Class<LocalDataStoreSlot>.NestedGenericInterface<string>))]
        public class TestInjection : Attribute, ICloneable, RuntimeAssets.CrossAssemblyHelpers.BaseGenericClass<int>.NestedGenericClass<string>.NestedGeneric2Class<LocalDataStoreSlot>.NestedGenericInterface<string>
        {
            public object Clone()
            {
                throw new NotImplementedException();
            }

            public string GetH<G>(G g, LocalDataStoreSlot i, string u, int t) where G : BaseGenericClass<int>.NestedGenericClass<string>.NestedGeneric2Class<LocalDataStoreSlot>.NestedGenericInterface<int>
            {
                throw new NotImplementedException();
            }

            [Advice(Kind.Before)]
            public void Test()
            {

            }
        }
    }
}
