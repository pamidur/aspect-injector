using AspectInjector.Core;
using AspectInjector.Core.Configuration;
using AspectInjector.Core.InterfaceProxy;
using AspectInjector.Core.MethodCall;
using Mono.Cecil;

namespace AspectInjector.CommandLine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ProcessingConfiguration.Default
                .RegisterAdviceExtractor<InterfaceAdviceExtractor>()
                .RegisterInjector<InterfaceInjector>()
                //.RegisterAdviceExtractor<MethodCallAdviceExtractor>()
                //.RegisterInjector<MethodCallInjector>()
                ;

            var processor = new Processor(config);

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(@"D:\tests\");

            processor.Process(@"D:\tests\AspectInjector.Tests.dll", resolver);
        }
    }
}