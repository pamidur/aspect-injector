using AspectInjector.Core;
using AspectInjector.Core.Configuration;
using Mono.Cecil;

namespace AspectInjector.CommandLine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ProcessingConfiguration.Default;
            var processor = new Processor(config);

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(@"D:\publish\infoaxs.mobile\bin\");

            processor.Process(@"D:\SVN\Git\aspect-injector\AspectInjector.Tests\bin\Debug\AspectInjector.Tests.dll", resolver);
        }
    }
}