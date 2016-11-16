using AspectInjector.Core;
using AspectInjector.Core.Configuration;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ProcessingConfiguration.Default;
            var processor = new Processor(config);

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(@"D:\publish\infoaxs.mobile\bin\");

            processor.Process(@"D:\publish\infoaxs.mobile\bin\Corp.InfoAXS.Mobile.Api.dll", resolver);
        }
    }
}