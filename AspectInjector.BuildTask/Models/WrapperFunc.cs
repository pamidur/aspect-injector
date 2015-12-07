using Mono.Cecil;

namespace AspectInjector.BuildTask.Models
{
    internal class WrapperFunc
    {
        public MethodReference MethodReference { get; private set; }

        public WrapperFunc(MethodReference mr)
        {
            MethodReference = mr;
        }
    }
}