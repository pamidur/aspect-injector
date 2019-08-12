using AspectInjector.Tests.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace AspectInjector.Tests.Runtime.General
{
    public class ReferencesTest
    {
        [Fact]
        public void NoRef_Runtime_To_PrivateCoreLib()
        {
            var refs = typeof(ReferencesTest).Assembly.GetReferencedAssemblies();
            Assert.DoesNotContain(refs, r => r.Name == "System.Private.CoreLib");
        }

        [Fact]
        public void NoRef_RuntimeAssets_To_PrivateCoreLib()
        {
            var refs = typeof(TestLog).Assembly.GetReferencedAssemblies();
            Assert.DoesNotContain(refs, r => r.Name == "System.Private.CoreLib");
        }
    }
}
