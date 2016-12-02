using AspectInjector.Core;
using AspectInjector.Core.Configuration;
using AspectInjector.Core.Mixin;
using Mono.Cecil;
using System;

namespace AspectInjector.CommandLine
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ProcessingConfiguration.Default
                .UseMixinInjections()
                //.RegisterAdviceExtractor<MethodCallAdviceExtractor>()
                //.RegisterInjector<MethodCallInjector>()
                ;

            var processor = new Processor(config);

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(@"C:\Users\Oleksandr.Hulyi\Documents\visual studio 2015\Projects\InterfacesTests\bin\Debug\");

            processor.Process(@"C:\Users\Oleksandr.Hulyi\Documents\visual studio 2015\Projects\InterfacesTests\bin\Debug\InterfacesTests.exe", resolver);

            Console.ReadKey();
        }
    }
}