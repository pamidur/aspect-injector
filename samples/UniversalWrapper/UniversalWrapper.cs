using AspectInjector.Broker;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace UniversalWrapper
{
    [Aspect(Scope.Global)]
    [Injection(typeof(UniversalWrapper))]
    public class UniversalWrapper : Attribute
    {
        private static readonly MethodInfo _asyncHandler = typeof(UniversalWrapper).GetMethod(nameof(UniversalWrapper.WrapAsync), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo _syncHandler = typeof(UniversalWrapper).GetMethod(nameof(UniversalWrapper.WrapSync), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly Type _voidTaskResult = Type.GetType("System.Threading.Tasks.VoidTaskResult");


        [Advice(Kind.Around)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Name)] string name,
            [Argument(Source.ReturnType)] Type retType
            )
        {
            if (typeof(Task).IsAssignableFrom(retType)) //check if method is async, you can also check by statemachine attribute
            {
                var syncResultType = retType.IsConstructedGenericType ? retType.GenericTypeArguments[0] : _voidTaskResult;
                var tgt = target;
                //if (!retType.IsConstructedGenericType)
                //    tgt = new Func<object[], object>(a=>((Task)target(a)).ContinueWith(t=> (object)null));
                return _asyncHandler.MakeGenericMethod(syncResultType).Invoke(this, new object[] { tgt, args, name });
            }
            else
            {
                retType = retType == typeof(void) ? typeof(object) : retType;
                return _syncHandler.MakeGenericMethod(retType).Invoke(this, new object[] { target, args, name });
            }
        }

        private static T WrapSync<T>(Func<object[], object> target, object[] args, string name)
        {
            try
            {
                var result = (T)target(args);
                Console.WriteLine($"Sync method `{name}` completes successfuly.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Sync method `{name}` throws {e.GetType()} exception.");
                return default;
            }
        }

        private static async Task<T> WrapAsync<T>(Func<object[], object> target, object[] args, string name)
        {
            try
            {
                var result = await ((Task<T>)target(args)).ConfigureAwait(false);
                Console.WriteLine($"Async method `{name}` completes successfuly.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Async method `{name}` throws {e.GetType()} exception.");
                return default;
            }
        }
    }
}
