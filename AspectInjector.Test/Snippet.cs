using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    public static class Snippet
    {
        public static Func<Type, object, object> CreateAspectFunc = null;
        public static Action<object> ReleaseAspect = null;

        static Snippet()
        {
            ReleaseAspect = ReleaseAspectDefault;
        }

        public static object CreateAspectDefault(Type type, object obj)
        {
            throw new NotImplementedException();
        }

        public static void ReleaseAspectDefault (object aspect)
        {
            if(aspect is IDisposable)
            {
                ((IDisposable)aspect).Dispose();
            }
        }

    }
}
