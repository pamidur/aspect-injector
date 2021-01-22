using System;
using System.Collections.Generic;
using System.Text;

namespace AspectInjector.Tests.RuntimeAssets.CrossAssemblyHelpers
{
    public class BaseGenericClass<T>
    {
        public class NestedGenericClass<U> : BaseGenericClass<U>
        {
            public class NestedGeneric2Class<I> : NestedGenericClass<I>
            {
                public interface NestedGenericInterface<H>
                {
                    H GetH<G,J>(G g, I i, U u, T t)
                        where G : NestedGenericInterface<J>
                        ;

                }
            }
        }
    }
}
